using System;
using Booker.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Booker.Services;

public class FavoritesManager
{
    private readonly DataContext _context;
    private readonly IMemoryCache _cache;

    public enum Status
    {
        Success,
        Error,
        NotFound,
        Forbidden,
        NotModified
    }

    public FavoritesManager(DataContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    private IQueryable<int> GetFavoriteIdsQueryable(int userId)
    {
        return _context.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Favorites.Select(f => f.Id))
            .AsQueryable();
    }

    public async Task<List<int>> GetFavoriteIdsAsync(int userId)
    {
        if (!_cache.TryGetValue("favorites" + userId, out List<int>? ids))
        {
            ids = await GetFavoriteIdsQueryable(userId)
                .ToListAsync();
            _cache.Set("favorites" + userId, ids, TimeSpan.FromHours(1));
        }

        return ids!;
    }

    private void InvalidateCache(int userId)
    {
        _cache.Remove("favorites" + userId);
    }
    
    public async Task<bool> IsFavoriteAsync(int userId, int itemId)
    {
        return (await GetFavoriteIdsAsync(userId))
            .Any(n => n == itemId);
    }

    public Task<Status> AddFavoriteAsync(int userId, int itemId)
        => ChangeFavoriteAsync(userId, itemId, true);

    public Task<Status> RemoveFavoriteAsync(int userId, int itemId)
        => ChangeFavoriteAsync(userId, itemId, false);

    private async Task<Status> ChangeFavoriteAsync(int userId, int itemId, bool isAdding)
    {
        var user = await _context.Users
            .Include(u => u.Favorites)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return Status.Forbidden;
        }

        var item = await _context.Items.FindAsync(itemId);

        if (item == null)
        {
            return Status.NotFound;
        }

        if (user.Favorites.Any(f => f.Id == itemId) == isAdding)
        {
            return Status.NotModified;
        }

        if (isAdding)
        {
            user.Favorites.Add(item);
        }
        else
        {
            user.Favorites.Remove(item);
        }

        await _context.SaveChangesAsync();
        InvalidateCache(userId);
        return Status.Success;
    }

    public async Task RemoveAllFavoritesAsync(int userId)
    {
        var user = await _context.Users.Include(u => u.Favorites).FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return;
        }

        user.Favorites.Clear();
        await _context.SaveChangesAsync();
        InvalidateCache(userId);
    }

}
