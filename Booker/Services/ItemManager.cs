using System;
using System.Drawing.Imaging;
using Booker.Data;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Booker.Services;

public class ItemManager
{
    private readonly DataContext _context;
    private readonly IMemoryCache _cache;

    public record Parameters(string? Search, Grade? Grade, Subject? Subject, bool? Level, decimal? MinPrice, decimal? MaxPrice);

    public ItemManager(DataContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
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
        return await GetAllItemsQueryable()
            .OrderByDescending(i => i.DateTime)
            .ToListAsync();
    }

    public async Task<int> GetAllItemsCountAsync()
    {
        return await GetAllItemsQueryable()
            .CountAsync();
    }

    public async Task<IEnumerable<Item>> GetItemsByParamsAsync(Parameters input)
    {
        var query = GetAllItemsQueryable();
        query = ApplyFilters(query, input);

        return await query
            .OrderByDescending(i => i.DateTime)
            .ToListAsync();
    }

    public async Task<int> GetItemsCountByParamsAsync(Parameters input)
    {
        var query = GetAllItemsQueryable();
        query = ApplyFilters(query, input);

        return await query
            .CountAsync();
    }

    public async Task<IEnumerable<Item>> GetPagedItemsByParamsAsync(Parameters input, int pageNumber, int pageSize)
    {
        var query = GetAllItemsQueryable();
        query = ApplyFilters(query, input);

        return await query
            .OrderByDescending(i => i.DateTime)
            .Skip(pageNumber * pageSize)
            .Take(pageSize)
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

    private IQueryable<Item> GetAllItemsQueryable()
    {
        return _context.Items
            .Include(i => i.Book).ThenInclude(b => b.Grades)
            .Include(i => i.Book).ThenInclude(b => b.Subject)
            .Include(i => i.User)
            .AsQueryable();
    }
    
    private static IQueryable<Item> ApplyFilters(IQueryable<Item> query, Parameters input)
    {
        query = ApplySearchFilter(query, input.Search);
        query = ApplyGradeFilter(query, input.Grade);
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

    private static IQueryable<Item> ApplyGradeFilter(IQueryable<Item> query, Grade? grade)
    {
        return grade == null
            ? query
            : query.Where(i => i.Book.Grades.Any(g => g == grade));
    }

    private static IQueryable<Item> ApplySubjectFilter(IQueryable<Item> query, Subject? subject)
    {
        return subject == null
            ? query
            : query.Where(i => i.Book.Subject == subject);
    }

    private static IQueryable<Item> ApplyPriceFilters(IQueryable<Item> query, decimal? minPrice, decimal? maxPrice)
    {
        return query.Where(i => !minPrice.HasValue || i.Price >= minPrice.Value)
                    .Where(i => !maxPrice.HasValue || i.Price <= maxPrice.Value);
    }

    private static IQueryable<Item> ApplyLevelFilter(IQueryable<Item> query, bool? level)
    {
        return level == null
            ? query
            : query.Where(i => i.Book.Level == level);
    }
}
