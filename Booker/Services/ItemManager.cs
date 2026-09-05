using Booker.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace Booker.Services;

public class ItemManager(DataContext context, StaticDataManager staticDataManager, PhotosManager photosManager, ILogger<ItemManager> logger)
{

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

    public record PagedItems(List<Item> Items, bool HasMorePages);
    public record ItemModel(
        User User,
        StaticDataManager.Parameters Parameters,
        string Description,
        string State,
        decimal Price,
        List<Stream>? ImageStreams = null,
        List<string>? ImageFileExtensions = null,
        string? ExistingImageFileNames = null
    );


    
    /// <summary>
    /// Gets an item by ID without school filtering. Use for admin scenarios only.
    /// </summary>
    public Task<Item?> GetItemAsync(int id) =>
        context.Items
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
        var item = await context.Items
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
        var query = GetAllItemsQueryable()
            .AsNoTracking();
        query = FilterByUserSchool(query, currentUser);
        
        return query
            .OrderByDescending(i => i.CreatedAt)
            .ThenBy(i => i.Id)
            .AsAsyncEnumerable();
    }

    public Task<int> GetAllItemsCountAsync(User? currentUser = null)
    {
        var query = GetAllItemsQueryable();
        query = FilterByUserSchool(query, currentUser);
        
        return query.CountAsync();
    }

    /// <summary>
    /// Pages the filtered listing entirely in SQL: filters, visibility, school isolation
    /// and newest-first ordering all run as a single query with OFFSET/FETCH paging.
    /// One extra row is fetched so a next page can be detected without a COUNT query.
    /// </summary>
    public Task<PagedItems> GetPagedItemsByParamsAsync(
        Parameters input,
        int pageNumber,
        int pageSize,
        User? currentUser = null,
        bool includeHidden = false)
        => GetPagedItemsCoreAsync(input, ids: null, pageNumber, pageSize, currentUser, includeHidden);

    public Task<PagedItems> GetPagedItemsByIdsAsync(
        IEnumerable<int> ids,
        int pageNumber,
        int pageSize,
        User? currentUser = null,
        bool includeHidden = false)
        => GetPagedItemsCoreAsync(input: null, ids, pageNumber, pageSize, currentUser, includeHidden);

    /// <summary>
    /// Returns the newest visible items for the landing page, honoring school isolation.
    /// </summary>
    public Task<List<Item>> GetRecentItemsAsync(int count, User? currentUser = null)
    {
        var query = GetAllItemsQueryable()
            .AsNoTracking();
        query = FilterByUserSchool(query, currentUser);

        return query
            .Where(i => i.IsVisible)
            .OrderByDescending(i => i.CreatedAt)
            .ThenBy(i => i.Id)
            .Take(count)
            .ToListAsync();
    }

    private async Task<PagedItems> GetPagedItemsCoreAsync(
        Parameters? input,
        IEnumerable<int>? ids,
        int pageNumber,
        int pageSize,
        User? currentUser,
        bool includeHidden)
    {
        var query = GetAllItemsQueryable()
            .AsNoTracking();
        query = FilterByUserSchool(query, currentUser);
        if (ids is not null)
        {
            query = query.Where(i => ids.Contains(i.Id));
        }
        if (input is not null)
        {
            query = ApplyFilters(query, input);
        }
        if (!includeHidden)
        {
            // Visibility must filter before paging, otherwise a page can end up
            // shorter than pageSize whenever hidden rows land inside it.
            query = query.Where(i => i.IsVisible);
        }

        var items = await query
            // Id breaks ties on CreatedAt; without it OFFSET/FETCH ordering is
            // non-deterministic and rows can repeat or vanish between pages.
            .OrderByDescending(i => i.CreatedAt)
            .ThenBy(i => i.Id)
            .Skip(pageNumber * pageSize)
            .Take(pageSize + 1)
            .ToListAsync();

        return new PagedItems(items.Take(pageSize).ToList(), items.Count > pageSize);
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

    public async Task TrackViewAsync(int itemId, int userId)
    {
        var alreadyViewed = await context.ItemViews
            .AnyAsync(v => v.ItemId == itemId && v.UserId == userId);

        if (alreadyViewed) return;

        context.ItemViews.Add(new ItemView { ItemId = itemId, UserId = userId });
        await context.SaveChangesAsync();
    }

    public Task<int> GetViewCountAsync(int itemId) =>
        context.ItemViews.CountAsync(v => v.ItemId == itemId);

    private async Task<Result> ValidateItemModelAsync(ItemModel model)
    {
        if (model.Parameters.Title == null
            || model.Parameters.Grades.IsNullOrEmpty()
            || model.Parameters.Subject == null
            || model.Parameters.Level == null)
            return Status.Error;

        var title = model.Parameters.Title;

        var books = await staticDataManager.GetBooksByTitleAsync(title);
        if (books.Count == 0) return Status.InvalidTitle;

        Status status = 0;

        var subjects = await staticDataManager.GetSubjectsByBookTitleAsync(title);
        if (!subjects.Contains(model.Parameters.Subject)) status |= Status.InvalidSubject | Status.Error;

        var grades = await staticDataManager.GetGradesByBookTitleAsync(title);
        if (!grades.SequenceEqual(model.Parameters.Grades)) status |= Status.InvalidGrades | Status.Error;

        var levels = await staticDataManager.GetLevelsByBookTitleAsync(title);
        if (!levels.Contains(model.Parameters.Level)) status |= Status.InvalidLevel | Status.Error;

        var book = (await staticDataManager.GetBooksByParamsAsync(model.Parameters)).FirstOrDefault();
        if (book == null) status |= Status.NotFound | Status.Error;

        if (status.HasFlag(Status.Error)) return status;

        return book!.Id;
    }

    public async Task<Result> AddItemAsync(ItemModel model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        var validationResult = await ValidateItemModelAsync(model);
        if (validationResult.Status.HasFlag(Status.Error)) return validationResult;

        var book = await context.Books.FindAsync(validationResult.Id);
        if (book == null) return Status.Error | Status.NotFound;

        string allPhotos = "";
        if (model.ImageStreams != null && model.ImageStreams.Count > 0)
        {
            if (!IsValidImagePayload(model.ImageStreams, model.ImageFileExtensions))
            {
                logger.LogWarning("Nieprawidłowy payload obrazów podczas dodawania ogłoszenia.");
                return Status.Error;
            }

            var photoFileNames = new List<string>();
            for (int i = 0; i < model.ImageStreams.Count; i++)
            {
                var fileName = await photosManager.AddPhotoAsync(model.ImageStreams[i], model.ImageFileExtensions![i]);
                photoFileNames.Add(fileName.ToString());
            }
            allPhotos = string.Join(";", photoFileNames);
        }
        else if (!string.IsNullOrEmpty(model.ExistingImageFileNames))
        {
            allPhotos = model.ExistingImageFileNames;
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
        context.Items.Add(item);
        await context.SaveChangesAsync();
        return item.Id;
    }

    
    public async Task<Status> UpdateItemAsync(Item item, ItemModel model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        var validationResult = await ValidateItemModelAsync(model);
        if (validationResult.Status.HasFlag(Status.Error)) return validationResult.Status;

        var book = await context.Books.FindAsync(validationResult.Id);
        if (book == null) return Status.Error | Status.NotFound;

        string allPhotos = model.ExistingImageFileNames ?? "";
        var uploadedPhotoUris = new List<string>();
        var shouldReplacePhotos = model.ImageStreams != null && model.ImageStreams.Count > 0;

        if (shouldReplacePhotos)
        {
            if (!IsValidImagePayload(model.ImageStreams, model.ImageFileExtensions))
            {
                logger.LogWarning("Nieprawidłowy payload obrazów podczas edycji ogłoszenia o ID {ItemId}.", item.Id);
                return Status.Error;
            }

            for (int i = 0; i < model.ImageStreams!.Count; i++)
            {
                var uri = await photosManager.AddPhotoAsync(model.ImageStreams[i], model.ImageFileExtensions![i]);
                uploadedPhotoUris.Add(uri.ToString());
            }

            allPhotos = string.Join(";", uploadedPhotoUris);
        }

        var oldPrice = item.Price;
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            item.Book = book;
            item.Description = model.Description;
            item.State = model.State;
            item.Price = model.Price;
            item.Photo = allPhotos;
            item.UpdatedAt = DateTime.Now;

            await UpdateItemNVAsync(item);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        if (oldPrice != item.Price)
        {
            logger.LogInformation("Cena ogłoszenia o ID {ItemId} użytkownika {UserName} została zmieniona z {OldPrice} zł na {NewPrice} zł.",
                item.Id, item.User.UserName, oldPrice, item.Price);
        }

        return Status.Success;
    }
    private async Task UpdateItemNVAsync(Item item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        context.Items.Update(item);
        await context.SaveChangesAsync();
    }

    public async Task DeleteItemAsync(int id)
    {
        var item = await GetItemAsync(id);
        if (item == null) return;

        // Only bare storage keys are deleted; seed and legacy items can also reference
        // root-relative assets or absolute URLs, which are not storage objects.
        var photoKeys = PhotosManager.StorageKeys(item.Photo).ToList();
        context.Items.Remove(item);
        await context.SaveChangesAsync();

        // Storage is cleaned up after the row is gone: a storage outage must not keep
        // the item alive, it only leaves orphaned objects that are logged for a purge.
        var orphanedKeys = await photosManager.DeletePhotosAsync(photoKeys);
        if (orphanedKeys.Count > 0)
        {
            logger.LogError("Item {ItemId} was deleted but its photo objects remain in storage. Orphaned keys: {OrphanedKeys}",
                item.Id, string.Join(", ", orphanedKeys));
        }
    }

    public async Task SetItemsVisibilityByUserAsync(int userId, bool isVisible)
    {
        var items = await context.Items
            .Where(i => i.UserId == userId && i.IsVisible != isVisible)
            .ToListAsync();

        foreach (var item in items)
        {
            item.IsVisible = isVisible;
        }

        if (items.Count > 0)
        {
            context.Items.UpdateRange(items);
            await context.SaveChangesAsync();
        }
    }

    private IQueryable<Item> GetAllItemsQueryable()
    {
        return context.Items
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

	public List<string> GetPhotosUrl(Item item)
	{
		return (item.Photo ?? "")
			.Split(';', StringSplitOptions.RemoveEmptyEntries)
			.Select(f => photosManager.GetPhotoUrl(f.Trim()))
			.ToList();
	}

    private static bool IsValidImagePayload(List<Stream>? imageStreams, List<string>? imageFileExtensions)
    {
        if (imageStreams == null || imageFileExtensions == null)
        {
            return false;
        }

        if (imageStreams.Count == 0 || imageStreams.Count != imageFileExtensions.Count)
        {
            return false;
        }

        for (int i = 0; i < imageStreams.Count; i++)
        {
            if (imageStreams[i] == null || !imageStreams[i].CanRead)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(imageFileExtensions[i]))
            {
                return false;
            }
        }

        return true;
    }
}
