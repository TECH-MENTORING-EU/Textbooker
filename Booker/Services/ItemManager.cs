using Booker.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace Booker.Services;

public class ItemManager
{
    private readonly DataContext _context;
    private readonly StaticDataManager _staticDataManager;
    private readonly PhotosManager _photosManager;
    private readonly ILogger<ItemManager> _logger;

    [Flags]
    public enum Status
    {
        Success = 0,
        Error = 1,
        InvalidTitle = 2,
        InvalidSubject = 4,
        InvalidGrades = 8,
        InvalidLevel = 16,
        NotFound = 32
    }

    public record Result(Status Status, int Id)
    {
        public static implicit operator Result(Status status) => new Result(status, -1);
        public static implicit operator Result(int Id) => new Result(Status.Success, Id);
    };
    
    public record Parameters(string? Search, List<Grade> Grades, Subject? Subject, Level? Level, decimal? MinPrice, decimal? MaxPrice);
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

    public ItemManager(DataContext context, StaticDataManager staticDataManager, PhotosManager photosManager, ILogger<ItemManager> logger)
    {
        _context = context;
        _staticDataManager = staticDataManager;
        _photosManager = photosManager;
        _logger = logger;
    }

    
    /// <summary>
    /// Gets an item by ID without school filtering. Use for admin scenarios only.
    /// </summary>
    public Task<Item?> GetItemAsync(int id) =>
        _context.Items
            .Include(i => i.Book).ThenInclude(b => b.Grades)
            .Include(i => i.Book).ThenInclude(b => b.Subject)
            .Include(i => i.Book).ThenInclude(b => b.Level)
            .Include(i => i.User).ThenInclude(u => u.School)
            .FirstOrDefaultAsync(i => i.Id == id);

    /// <summary>
    /// Gets an item by ID with school isolation filtering.
    /// Returns null if item doesn't exist or user doesn't have access to it (wrong school).
    /// </summary>
    public async Task<Item?> GetItemAsync(int id, User? currentUser)
    {
        var item = await _context.Items
            .Include(i => i.Book).ThenInclude(b => b.Grades)
            .Include(i => i.Book).ThenInclude(b => b.Subject)
            .Include(i => i.Book).ThenInclude(b => b.Level)
            .Include(i => i.User).ThenInclude(u => u.School)
            .FirstOrDefaultAsync(i => i.Id == id);
        
        if (item == null) return null;
        
        // Apply school isolation
        if (currentUser == null)
        {
            // Anonymous users can see items from all active schools
            return item;
        }
        
        if (currentUser.SchoolId.HasValue)
        {
            // User with school can only see items from their own school
            if (item.User.SchoolId != currentUser.SchoolId.Value)
            {
                return null;
            }
        }
        else
        {
            // User without school can only see items from users without a school
            if (item.User.SchoolId != null)
            {
                return null;
            }
        }
        
        return item;
    }

    public IAsyncEnumerable<Item> GetAllItemsAsync(User? currentUser = null)
    {
        var query = GetAllItemsQueryable();
        query = FilterByUserSchool(query, currentUser);
        
        return query
            .OrderByDescending(i => i.CreatedAt)
            .AsAsyncEnumerable();
    }

    public Task<int> GetAllItemsCountAsync(User? currentUser = null)
    {
        var query = GetAllItemsQueryable();
        query = FilterByUserSchool(query, currentUser);
        
        return query.CountAsync();
    }

    public IAsyncEnumerable<Item> GetItemsByIdsAsync(IEnumerable<int> ids, User? currentUser = null)
    {
        var query = GetAllItemsQueryable();
        query = FilterByUserSchool(query, currentUser);
        
        return query
            .Where(i => ids.Contains(i.Id))
            .OrderByDescending(i => i.CreatedAt)
            .AsAsyncEnumerable();
    }

    public IAsyncEnumerable<Item> GetPagedItemsByIdsAsync(IEnumerable<int> ids, int pageNumber, int pageSize, User? currentUser = null)
    {
        var query = GetAllItemsQueryable();
        query = FilterByUserSchool(query, currentUser);
        
        return query
            .Where(i => ids.Contains(i.Id))
            .OrderByDescending(i => i.CreatedAt)
            .Skip(pageNumber * pageSize)
            .Take(pageSize)
            .AsAsyncEnumerable();
    }

    public IAsyncEnumerable<int> GetItemIdsByParamsAsync(Parameters input, User? currentUser = null)
    {
        var query = GetAllItemsQueryable();
        query = FilterByUserSchool(query, currentUser);
        query = ApplyFilters(query, input);

        return query
            .Select(i => i.Id)
            .AsAsyncEnumerable();
    }

    public Task<int> GetItemsCountByParamsAsync(Parameters input, User? currentUser = null)
    {
        var query = GetAllItemsQueryable();
        query = FilterByUserSchool(query, currentUser);
        query = ApplyFilters(query, input);

        return query.CountAsync();
    }

    public IAsyncEnumerable<int> GetUserItemIdsAsync(int userId)
    {
        return GetAllItemsQueryable()
            .Where(i => i.UserId == userId)
            .Select(i => i.Id)
            .AsAsyncEnumerable();
    }

    public Task<int> GetUserItemsCountAsync(int userId)
    {
        return GetAllItemsQueryable()
            .Where(i => i.UserId == userId)
            .CountAsync();
    }

    public async Task MarkItemReservedAsync(int itemId, bool reserved)
    {
        var item = await GetItemAsync(itemId);
        item!.Reserved = reserved;

        await UpdateItemNVAsync(item!);
    }
    private async Task<Result> ValidateItemModelAsync(ItemModel model)
    {
        if (model.Parameters.Title == null
            || model.Parameters.Grades.IsNullOrEmpty()
            || model.Parameters.Subject == null
            || model.Parameters.Level == null)
            return Status.Error;

        var title = model.Parameters.Title;

        var books = await _staticDataManager.GetBooksByTitleAsync(title);
        if (books.Count == 0) return Status.InvalidTitle;

        Status status = 0;

        var subjects = await _staticDataManager.GetSubjectsByBookTitleAsync(title);
        if (!subjects.Contains(model.Parameters.Subject)) status |= Status.InvalidSubject | Status.Error;

        var grades = await _staticDataManager.GetGradesByBookTitleAsync(title);
        if (!grades.SequenceEqual(model.Parameters.Grades)) status |= Status.InvalidGrades | Status.Error;

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

        var book = await _context.Books.FindAsync(validationResult.Id);
        if (book == null) return Status.Error | Status.NotFound;

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
            CreatedAt = DateTime.Now,
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

        var book = await _context.Books.FindAsync(validationResult.Id);
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

        var oldPrice = item.Price;
        item.Book = book;
        item.Description = model.Description;
        item.State = model.State;
        item.Price = model.Price;
        item.Photo = allPhotos;
        item.UpdatedAt = DateTime.Now;

        await UpdateItemNVAsync(item);
        if (oldPrice != item.Price)
        {
            _logger.LogInformation($"Cena ogłoszenia o ID {item.Id} użytkownika {item.User.UserName} została zmieniona z {oldPrice} zł na {item.Price} zł.");
        }

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

    public async Task SetItemsVisibilityByUserAsync(int userId, bool isVisible)
    {
        var items = await _context.Items
            .Where(i => i.UserId == userId && i.IsVisible != isVisible)
            .ToListAsync();

        foreach (var item in items)
        {
            item.IsVisible = isVisible;
        }

        if (items.Count > 0)
        {
            _context.Items.UpdateRange(items);
            await _context.SaveChangesAsync();
        }
    }

    private IQueryable<Item> GetAllItemsQueryable()
    {
        return _context.Items
            .Include(i => i.Book).ThenInclude(b => b.Grades)
            .Include(i => i.Book).ThenInclude(b => b.Subject)
            .Include(i => i.Book).ThenInclude(b => b.Level)
            .Include(i => i.User).ThenInclude(u => u.School)
            .AsQueryable();
    }
    
    /// <summary>
    /// Filters items to only show those from users in the same school as the given user.
    /// If the user has no school assigned, returns all items from users without a school.
    /// </summary>
    private static IQueryable<Item> FilterByUserSchool(IQueryable<Item> query, User? currentUser)
    {
        if (currentUser == null)
        {
            // Anonymous users see items from all schools
            return query;
        }

        return currentUser.SchoolId.HasValue
            // Show only items from users in the same school
            ? query.Where(i => i.User.SchoolId == currentUser.SchoolId.Value)
            // User has no school - show items from users without a school
            : query.Where(i => i.User.SchoolId == null);
    }
    
    private static IQueryable<Item> ApplyFilters(IQueryable<Item> query, Parameters input)
    {
        query = ApplySearchFilter(query, input.Search);
        query = ApplyGradesFilter(query, input.Grades);
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

    private static IQueryable<Item> ApplyGradesFilter(IQueryable<Item> query, List<Grade> grades)
    {
        return grades.IsNullOrEmpty()
            ? query
            : query.Where(i => i.Book.Grades.Any(g => grades.Contains(g)));
    }

    private static IQueryable<Item> ApplySubjectFilter(IQueryable<Item> query, Subject? subject)
    {
        return subject == null
            ? query
            : query.Where(i => i.Book.Subject.Id == subject.Id);
    }

    private static IQueryable<Item> ApplyPriceFilters(IQueryable<Item> query, decimal? minPrice, decimal? maxPrice)
    {
        return query.Where(i => !minPrice.HasValue || i.Price >= minPrice.Value)
                    .Where(i => !maxPrice.HasValue || i.Price <= maxPrice.Value);
    }

    private static IQueryable<Item> ApplyLevelFilter(IQueryable<Item> query, Level? level)
    {
        return level == null
            ? query
            : query.Where(i => i.Book.Level.Id == level.Id);
    }
}
