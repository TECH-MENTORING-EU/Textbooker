using System;
using System.Drawing.Imaging;
using Booker.Data;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Booker.Services;

public class ItemManager
{
    private readonly DataContext _context;
    private readonly IMemoryCache _cache;
    private readonly StaticDataManager _staticDataManager;
    private readonly PhotosManager _photosManager;

    [Flags]
    public enum Status
    {
        Success = 0,
        Error = 1,
        InvalidTitle = 2,
        InvalidSubject = 4,
        InvalidGrade = 8,
        InvalidLevel = 16,
        NotFound = 32
    }

    public record Result(Status Status, int Id)
    {
        public static implicit operator Result(Status status) => new Result(status, -1);
        public static implicit operator Result(int Id) => new Result(Status.Success, Id);
    };
    


    public record Parameters(string? Search, Grade? Grade, Subject? Subject, bool? Level, decimal? MinPrice, decimal? MaxPrice);
    public record ItemModel(
        User User,
        StaticDataManager.Parameters Parameters,
        string Description,
        string State,
        decimal Price,
        Stream? ImageStream,
        string? ImageFileExtension,
        string? ExistingImageBlobName = null
    );

    public ItemManager(DataContext context, IMemoryCache cache, StaticDataManager staticDataManager, PhotosManager photosManager)
    {
        _context = context;
        _cache = cache;
        _staticDataManager = staticDataManager;
        _photosManager = photosManager;
    }

    public async Task<Item?> GetItemAsync(int id)
    {
        return await _context.Items
            .Include(i => i.Book).ThenInclude(b => b.Grades)
            .Include(i => i.Book).ThenInclude(b => b.Subject)
            .Include(i => i.User)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IEnumerable<Item>> GetAllItemsAsync()
    {
        return await GetAllItemsQueryable()
            .OrderByDescending(i => i.DateTime)
            .ToListAsync();
    }

    public async Task<int> GetAllItemsCountAsync()
    {
        return await GetAllItemsQueryable()
            .CountAsync();
    }

    public async Task<IEnumerable<Item>> GetItemsByIdsAsync(IEnumerable<int> ids)
    {
        return await GetAllItemsQueryable()
            .Where(i => ids.Contains(i.Id))
            .OrderByDescending(i => i.DateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Item>> GetPagedItemsByIdsAsync(IEnumerable<int> ids, int pageNumber, int pageSize)
    {
        return await GetAllItemsQueryable()
            .Where(i => ids.Contains(i.Id))
            .OrderByDescending(i => i.DateTime)
            .Skip(pageNumber * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }       

    public async Task<IEnumerable<Item>> GetItemsByParamsAsync(Parameters input)
    {
        var query = GetAllItemsQueryable();
        query = ApplyFilters(query, input);

        return await query
            .OrderByDescending(i => i.DateTime)
            .ToListAsync();
    }

    public async Task<int> GetItemsCountByParamsAsync(Parameters input)
    {
        var query = GetAllItemsQueryable();
        query = ApplyFilters(query, input);

        return await query
            .CountAsync();
    }

    public async Task<IEnumerable<Item>> GetPagedItemsByParamsAsync(Parameters input, int pageNumber, int pageSize)
    {
        var query = GetAllItemsQueryable();
        query = ApplyFilters(query, input);

        return await query
            .OrderByDescending(i => i.DateTime)
            .Skip(pageNumber * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Item>> GetUserItemsAsync(int userId)
    {
        return await GetAllItemsQueryable()
            .Where(i => i.UserId == userId)
            .OrderByDescending(i => i.DateTime)
            .ToListAsync();
    }

    public async Task<int> GetUserItemsCountAsync(int userId)
    {
        return await GetAllItemsQueryable()
            .Where(i => i.UserId == userId)
            .CountAsync();
    }

    public async Task<IEnumerable<Item>> GetPagedUserItemsAsync(int userId, int pageNumber, int pageSize)
    {
        return await GetAllItemsQueryable()
            .Where(i => i.UserId == userId)
            .OrderByDescending(i => i.DateTime)
            .Skip(pageNumber * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    private async Task<Result> ValidateItemModelAsync(ItemModel model)
    {
        if (model.Parameters.Title == null
            || model.Parameters.Grade == null
            || model.Parameters.Subject == null)
            return Status.Error;

        var title = model.Parameters.Title;

        var books = await _staticDataManager.GetBooksByTitleAsync(title);
        if (books.Count == 0) return Status.InvalidTitle;

        Status status = 0;

        var subjects = await _staticDataManager.GetSubjectsByBookTitleAsync(title);
        if (!subjects.Contains(model.Parameters.Subject)) status |= Status.InvalidSubject | Status.Error;

        var grades = await _staticDataManager.GetGradesByBookTitleAsync(title);
        if (!grades.Contains(model.Parameters.Grade)) status |= Status.InvalidGrade | Status.Error;

        var levels = await _staticDataManager.GetLevelsByBookTitleAsync(title);
        if (!levels.Contains(model.Parameters.Level)) status |= Status.InvalidLevel | Status.Error;

        var book = (await _staticDataManager.GetBooksByParamsAsync(model.Parameters)).FirstOrDefault();
        if (book == null) status |= Status.NotFound | Status.Error;

        if (status.HasFlag(Status.Error)) return status;

        return book!.Id;
    }

    public async Task<Result> AddItemAsync(ItemModel model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        var validationResult = await ValidateItemModelAsync(model);
        if (validationResult.Status.HasFlag(Status.Error)) return validationResult;     

        var photoUri = await _photosManager.AddPhotoAsync(model.ImageStream!, model.ImageFileExtension!);
        var item = new Item
        {
            Book = (await _staticDataManager.GetBookAsync(validationResult.Id))!,
            User = model.User,
            Description = model.Description,
            State = model.State,
            Price = model.Price,
            DateTime = DateTime.Now,
            Photo = photoUri.ToString()
        };

        return await AddItemNVAsync(item);
    }

    private async Task<int> AddItemNVAsync(Item item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        _context.Items.Add(item);
        await _context.SaveChangesAsync();
        return item.Id;
    }

    public async Task<Status> UpdateItemAsync(int id, ItemModel model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        var validationResult = await ValidateItemModelAsync(model);
        if (validationResult.Status.HasFlag(Status.Error)) return validationResult.Status;

        var photoUri = model.ExistingImageBlobName;

        if (model.ImageStream != null)
        {
            await _photosManager.DeletePhotoAsync(model.ExistingImageBlobName!);
            photoUri = (await _photosManager.AddPhotoAsync(model.ImageStream!, model.ImageFileExtension!)).ToString();
        }

        var item = new Item
        {
            Id = id,
            Book = (await _staticDataManager.GetBookAsync(validationResult.Id))!,
            User = model.User,
            Description = model.Description,
            State = model.State,
            Price = model.Price,
            DateTime = DateTime.Now,
            Photo = photoUri!
        };
        await UpdateItemNVAsync(item);
        return Status.Success;
    }
    private async Task UpdateItemNVAsync(Item item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        _context.Items.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteItemAsync(int id)
    {
        var item = await GetItemAsync(id);
        if (item == null) return;
        await _photosManager.DeletePhotoAsync(item.Photo);
        _context.Items.Remove(item);
        await _context.SaveChangesAsync();
    }

    private IQueryable<Item> GetAllItemsQueryable()
    {
        return _context.Items
            .Include(i => i.Book).ThenInclude(b => b.Grades)
            .Include(i => i.Book).ThenInclude(b => b.Subject)
            .Include(i => i.User)
            .AsQueryable();
    }
    
    private static IQueryable<Item> ApplyFilters(IQueryable<Item> query, Parameters input)
    {
        query = ApplySearchFilter(query, input.Search);
        query = ApplyGradeFilter(query, input.Grade);
        query = ApplySubjectFilter(query, input.Subject);
        query = ApplyPriceFilters(query, input.MinPrice, input.MaxPrice);
        query = ApplyLevelFilter(query, input.Level);

        return query;
    }

    private static IQueryable<Item> ApplySearchFilter(IQueryable<Item> query, string? search)
    {
        return string.IsNullOrWhiteSpace(search)
            ? query
            : query.Where(i => i.Book.Title.Contains(search.ToLower()));
    }

    private static IQueryable<Item> ApplyGradeFilter(IQueryable<Item> query, Grade? grade)
    {
        return grade == null
            ? query
            : query.Where(i => i.Book.Grades.Any(g => g == grade));
    }

    private static IQueryable<Item> ApplySubjectFilter(IQueryable<Item> query, Subject? subject)
    {
        return subject == null
            ? query
            : query.Where(i => i.Book.Subject == subject);
    }

    private static IQueryable<Item> ApplyPriceFilters(IQueryable<Item> query, decimal? minPrice, decimal? maxPrice)
    {
        return query.Where(i => !minPrice.HasValue || i.Price >= minPrice.Value)
                    .Where(i => !maxPrice.HasValue || i.Price <= maxPrice.Value);
    }

    private static IQueryable<Item> ApplyLevelFilter(IQueryable<Item> query, bool? level)
    {
        return level == null
            ? query
            : query.Where(i => i.Book.Level == level);
    }
}
