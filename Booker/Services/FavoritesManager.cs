using System;
using Booker.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Booker.Services;

public class FavoritesManager(DataContext context, ItemManager itemManager, IMemoryCache cache)
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
        // Race-safe strategy: instead of loading the full Favorites collection and
        // mutating it in memory (two concurrent requests both see the same snapshot
        // → duplicate row or DbUpdateException), the change is a set-based SQL
        // command — correct under any interleaving.
        var userExists = await context.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            return Status.Forbidden;
        }

        if (isAdding)
        {
            // Adding requires the same access the offer page enforces: school isolation
            // (GetItemAsync answers cross-school and missing ids alike with null) plus
            // item visibility. One deliberate difference from the offer page: only the
            // owner may favorite a hidden item. Admins can view hidden offers, but a
            // favorite is a personal bookmark, not an admin capability.
            var user = await context.Users.FirstAsync(u => u.Id == userId);
            var item = await itemManager.GetItemAsync(itemId, user);

            if (item == null || (!item.IsVisible && item.UserId != userId))
            {
                return Status.NotFound;
            }

            var alreadyFavorite = await context.Users
                .Where(u => u.Id == userId)
                .SelectMany(u => u.Favorites.Where(f => f.Id == itemId))
                .AnyAsync();

            if (alreadyFavorite)
            {
                return Status.NotModified;
            }

            // Idempotent insert: a row added concurrently is a no-op, so two racing
            // requests cannot create duplicates or throw.
            await context.Database.ExecuteSqlAsync($"""
                IF NOT EXISTS (SELECT 1 FROM Favorites WHERE UserId = {userId} AND ItemId = {itemId})
                    INSERT INTO Favorites (UserId, ItemId) VALUES ({userId}, {itemId})
                """);
        }
        else
        {
            // Removal stays unrestricted so favorites that no longer pass the checks
            // above (hidden item, cross-school owner) can still be removed.
            var itemExists = await context.Items.AnyAsync(i => i.Id == itemId);

            if (!itemExists)
            {
                return Status.NotFound;
            }

            var isFavorite = await context.Users
                .Where(u => u.Id == userId)
                .SelectMany(u => u.Favorites.Where(f => f.Id == itemId))
                .AnyAsync();

            if (!isFavorite)
            {
                return Status.NotModified;
            }

            // Set-based delete: idempotent under concurrent removals.
            await context.Database.ExecuteSqlAsync($"""
                DELETE FROM Favorites WHERE UserId = {userId} AND ItemId = {itemId}
                """);
        }

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
