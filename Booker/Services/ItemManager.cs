using Booker.Data;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Booker.Services;

public class ItemManager
{
    private readonly DataContext _context;
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
        List<Stream>? ImageStreams = null,
        List<string>? ImageFileExtensions = null,
        string? ExistingImageBlobNames = null
    );

    public ItemManager(DataContext context, StaticDataManager staticDataManager, PhotosManager photosManager)
    {
        _context = context;
        _staticDataManager = staticDataManager;
        _photosManager = photosManager;
    }

    
    public Task<Item?> GetItemAsync(int id) =>
        _context.Items
            .Include(i => i.Book).ThenInclude(b => b.Grades)
            .Include(i => i.Book).ThenInclude(b => b.Subject)
            .Include(i => i.User)
            .FirstOrDefaultAsync(i => i.Id == id);

    public IAsyncEnumerable<Item> GetAllItemsAsync() =>
        GetAllItemsQueryable().OrderByDescending(i => i.DateTime).AsAsyncEnumerable();

    public Task<int> GetAllItemsCountAsync() => GetAllItemsQueryable().CountAsync();

    public IAsyncEnumerable<Item> GetItemsByIdsAsync(IEnumerable<int> ids) =>
        GetAllItemsQueryable().Where(i => ids.Contains(i.Id)).OrderByDescending(i => i.DateTime).AsAsyncEnumerable();

    public IAsyncEnumerable<Item> GetPagedItemsByIdsAsync(IEnumerable<int> ids, int pageNumber, int pageSize) =>
        GetAllItemsQueryable().Where(i => ids.Contains(i.Id))
                              .OrderByDescending(i => i.DateTime)
                              .Skip(pageNumber * pageSize)
                              .Take(pageSize)
                              .AsAsyncEnumerable();

    public IAsyncEnumerable<int> GetItemIdsByParamsAsync(Parameters input) =>
        ApplyFilters(GetAllItemsQueryable(), input).Select(i => i.Id).AsAsyncEnumerable();

    public Task<int> GetItemsCountByParamsAsync(Parameters input) =>
        ApplyFilters(GetAllItemsQueryable(), input).CountAsync();

    public IAsyncEnumerable<int> GetUserItemIdsAsync(int userId) =>
        GetAllItemsQueryable().Where(i => i.UserId == userId).Select(i => i.Id).AsAsyncEnumerable();

    public Task<int> GetUserItemsCountAsync(int userId) =>
        GetAllItemsQueryable().Where(i => i.UserId == userId).CountAsync();

    
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

        var book = await _staticDataManager.GetBookAsync(validationResult.Id);
        if (book == null) return Status.Error | Status.NotFound;

        _context.Attach(book);
        _context.Attach(model.User);

        string allPhotos = "";
        if (model.ImageStreams != null && model.ImageStreams.Count > 0)
        {
            var photoUris = new List<string>();
            for (int i = 0; i < model.ImageStreams.Count; i++)
            {
                var uri = await _photosManager.AddPhotoAsync(model.ImageStreams[i], model.ImageFileExtensions![i]);
                photoUris.Add(uri.ToString());
            }
            allPhotos = string.Join(";", photoUris);
        }
        else if (!string.IsNullOrEmpty(model.ExistingImageBlobNames))
        {
            allPhotos = model.ExistingImageBlobNames;
        }

        var item = new Item
        {
            Book = book,
            User = model.User,
            Description = model.Description,
            State = model.State,
            Price = model.Price,
            DateTime = DateTime.Now,
            Photo = allPhotos
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

    
    public async Task<Status> UpdateItemAsync(Item item, ItemModel model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        var validationResult = await ValidateItemModelAsync(model);
        if (validationResult.Status.HasFlag(Status.Error)) return validationResult.Status;

        var book = await _staticDataManager.GetBookAsync(validationResult.Id);
        if (book == null) return Status.Error | Status.NotFound;

        string allPhotos = model.ExistingImageBlobNames ?? "";

        if (model.ImageStreams != null && model.ImageStreams.Count > 0)
        {
            if (!string.IsNullOrEmpty(model.ExistingImageBlobNames))
            {
                var oldPhotos = model.ExistingImageBlobNames.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var photo in oldPhotos)
                    await _photosManager.DeletePhotoAsync(photo);
            }

            var photoUris = new List<string>();
            for (int i = 0; i < model.ImageStreams.Count; i++)
            {
                var uri = await _photosManager.AddPhotoAsync(model.ImageStreams[i], model.ImageFileExtensions![i]);
                photoUris.Add(uri.ToString());
            }
            allPhotos = string.Join(";", photoUris);
        }

        if (item.Book.Id != book.Id)
        {
            _context.Attach(book);
            item.Book = book;
        }

        item.Description = model.Description;
        item.State = model.State;
        item.Price = model.Price;
        item.Photo = allPhotos;
        item.DateTime = DateTime.Now;

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

        if (!string.IsNullOrEmpty(item.Photo))
        {
            var oldPhotos = item.Photo.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var photo in oldPhotos)
                await _photosManager.DeletePhotoAsync(photo);
        }

        _context.Items.Remove(item);
        await _context.SaveChangesAsync();
    }

    
    private IQueryable<Item> GetAllItemsQueryable() =>
        _context.Items
            .Include(i => i.Book).ThenInclude(b => b.Grades)
            .Include(i => i.Book).ThenInclude(b => b.Subject)
            .Include(i => i.User)
            .AsQueryable();

    private static IQueryable<Item> ApplyFilters(IQueryable<Item> query, Parameters input)
    {
        query = ApplySearchFilter(query, input.Search);
        query = ApplyGradeFilter(query, input.Grade);
        query = ApplySubjectFilter(query, input.Subject);
        query = ApplyPriceFilters(query, input.MinPrice, input.MaxPrice);
        query = ApplyLevelFilter(query, input.Level);
        return query;
    }

    private static IQueryable<Item> ApplySearchFilter(IQueryable<Item> query, string? search) =>
        string.IsNullOrWhiteSpace(search) ? query : query.Where(i => i.Book.Title.Contains(search.ToLower()));

    private static IQueryable<Item> ApplyGradeFilter(IQueryable<Item> query, Grade? grade) =>
        grade == null ? query : query.Where(i => i.Book.Grades.Any(g => g.Id == grade.Id));

    private static IQueryable<Item> ApplySubjectFilter(IQueryable<Item> query, Subject? subject) =>
        subject == null ? query : query.Where(i => i.Book.Subject.Id == subject.Id);

    private static IQueryable<Item> ApplyPriceFilters(IQueryable<Item> query, decimal? minPrice, decimal? maxPrice) =>
        query.Where(i => !minPrice.HasValue || i.Price >= minPrice.Value)
             .Where(i => !maxPrice.HasValue || i.Price <= maxPrice.Value);

    private static IQueryable<Item> ApplyLevelFilter(IQueryable<Item> query, bool? level) =>
        level == null ? query : query.Where(i => i.Book.Level == level);
}
