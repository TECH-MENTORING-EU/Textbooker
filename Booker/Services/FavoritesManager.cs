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

    public async Task<IEnumerable<int>> GetFavoriteIdsAsync(int userId)
    {
        return await GetFavoriteIdsQueryable(userId)
            .ToListAsync();
    }

    public async Task<bool> IsFavoriteAsync(int userId, int itemId)
    {
        return await GetFavoriteIdsQueryable(userId)
            .AnyAsync(n => n == itemId);
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
    }

}
