using System;
using Booker.Data;
using Microsoft.EntityFrameworkCore;

namespace Booker.Services;

public class ItemManager
{
    private readonly DataContext _context;

    public ItemManager(DataContext context)
    {
        _context = context;
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
        return await _context.Items
            .Include(i => i.Book).ThenInclude(b => b.Grades)
            .Include(i => i.Book).ThenInclude(b => b.Subject)
            .Include(i => i.User)
            .ToListAsync();
    }

    public async Task AddItemAsync(Item item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        _context.Items.Add(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateItemAsync(Item item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        _context.Items.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteItemAsync(int id)
    {
        var item = await GetItemAsync(id);
        if (item == null) return;
        _context.Items.Remove(item);
        await _context.SaveChangesAsync();
    }
}
