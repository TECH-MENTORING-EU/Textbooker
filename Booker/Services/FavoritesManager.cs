using System;
using Booker.Data;
using Microsoft.EntityFrameworkCore;

namespace Booker.Services;

public class FavoritesManager
{
    private readonly DataContext _context;
    public FavoritesManager(DataContext context)
    {
        _context = context;
    }
    
    public async Task<bool> IsFavoriteAsync(int userId, int itemId)
    {
        return await _context.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Favorites.Select(f => f.Id))
            .AnyAsync(n => n == itemId);
    }
}
