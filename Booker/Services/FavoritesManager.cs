using System;
using Booker.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Booker.Services;

public class FavoritesManager(DataContext context, IMemoryCache cache)
{

    public enum Status
    {
        Success,
        Error,
        NotFound,
        Forbidden,
        NotModified
    }

    private IQueryable<int> GetFavoriteIdsQueryable(int userId)
    {
        return context.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Favorites.Select(f => f.Id))
            .AsQueryable();
    }

    public async Task<List<int>> GetFavoriteIdsAsync(int userId)
    {
        if (!cache.TryGetValue("favorites" + userId, out List<int>? ids))
        {
            ids = await GetFavoriteIdsQueryable(userId)
                .ToListAsync();
            cache.Set("favorites" + userId, ids, TimeSpan.FromHours(1));
        }

        return ids!;
    }

    private void InvalidateCache(int userId)
    {
        cache.Remove("favorites" + userId);
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
        var user = await context.Users
            .Include(u => u.Favorites)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return Status.Forbidden;
        }

        var item = await context.Items.FindAsync(itemId);

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

        await context.SaveChangesAsync();
        InvalidateCache(userId);
        return Status.Success;
    }

    public async Task RemoveAllFavoritesAsync(int userId)
    {
        var user = await context.Users.Include(u => u.Favorites).FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return;
        }

        user.Favorites.Clear();
        await context.SaveChangesAsync();
        InvalidateCache(userId);
    }

}
