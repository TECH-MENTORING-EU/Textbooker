using System;
using Booker.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Booker.Services;

public class FavoritesManager
{
    private readonly DataContext _context;
    private readonly IMemoryCache _cache;

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
}
