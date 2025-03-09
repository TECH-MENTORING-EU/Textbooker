using Booker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Booker.TagHelpers;
using Microsoft.Extensions.Caching.Memory;

namespace Booker.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly DataContext _context;
        private readonly IMemoryCache _cache;
        const int PageSize = 25;

        public record PagedListViewModel(List<Item> Items, int Page, bool HasMorePages);
        public record FilterParameters(int PageNumber, string? Search, string? Grade, string? Subject, decimal? MinPrice, decimal? MaxPrice, string? Level);

        public PagedListViewModel? ItemsList { get; set; }
        public List<string>? Grades { get; set; }
        public List<string>? Subjects { get; set; }

        public IndexModel(ILogger<IndexModel> logger, DataContext context, IMemoryCache cache)
        {
            _logger = logger;
            _context = context;
            _cache = cache;
        }

        public async Task<IActionResult> OnGetAsync([FromQuery] FilterParameters parameters)
        {
            if (!_cache.TryGetValue("grades", out List<string>? grades))
            {
                grades = await _context.Grades
                    .OrderBy(g => g.Id)
                    .Select(g => g.GradeNumber)
                    .ToListAsync();
                _cache.Set("grades", grades, TimeSpan.FromHours(1));
            }

            Grades = grades;

            if (!_cache.TryGetValue("subjects", out List<string>? subjects))
            {
                subjects = await _context.Subjects
                    .OrderBy(s => s.Name)
                    .Select(s => s.Name)
                    .ToListAsync();
                _cache.Set("subjects", subjects, TimeSpan.FromHours(1));
            }

            Subjects = subjects;

            var query = _context.Items
                .Include(i => i.Book).ThenInclude(b => b.Grades)
                .Include(i => i.Book).ThenInclude(b => b.Subject)
                .Include(i => i.User)
                .AsQueryable();

            query = ApplyFilters(query, parameters);

            var totalItems = await query.CountAsync();
            bool hasMorePages = totalItems > (parameters.PageNumber + 1) * PageSize;

            var items = await query
                .OrderBy(i => i.DateTime)
                .Skip(parameters.PageNumber * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ItemsList = new PagedListViewModel(items, parameters.PageNumber, hasMorePages);

            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return Partial("_ItemGallery", ItemsList);
            }
            return Page();
        }

        public async Task<IActionResult> OnGetMoreAsync([FromQuery] FilterParameters parameters)
        {
            var query = _context.Items
                .Include(i => i.Book).ThenInclude(b => b.Grades)
                .Include(i => i.Book).ThenInclude(b => b.Subject)
                .Include(i => i.User)
                .AsQueryable();

            query = ApplyFilters(query, parameters);

            var totalItems = await query.CountAsync();
            bool hasMorePages = totalItems > (parameters.PageNumber + 1) * PageSize;

            var items = await query
                .OrderBy(i => i.DateTime)
                .Skip(parameters.PageNumber * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var pagedListViewModel = new PagedListViewModel(items, parameters.PageNumber, hasMorePages);

            return Partial("_ItemGallery", pagedListViewModel);
        }

        private IQueryable<Item> ApplyFilters(IQueryable<Item> query, FilterParameters parameters)
        {
            query = ApplySearchFilter(query, parameters.Search);
            query = ApplyGradeFilter(query, parameters.Grade);
            query = ApplySubjectFilter(query, parameters.Subject);
            query = ApplyPriceFilters(query, parameters.MinPrice, parameters.MaxPrice);
            query = ApplyLevelFilter(query, parameters.Level);

            return query;
        }

        private IQueryable<Item> ApplySearchFilter(IQueryable<Item> query, string? search)
        {
            return string.IsNullOrWhiteSpace(search)
                ? query
                : query.Where(i => i.Book.Title.Contains(search.ToLower()));
        }

        private IQueryable<Item> ApplyGradeFilter(IQueryable<Item> query, string? grade)
        {
            return string.IsNullOrWhiteSpace(grade)
                ? query
                : query.Where(i => i.Book.Grades.Any(g => g.GradeNumber == grade));
        }

        private IQueryable<Item> ApplySubjectFilter(IQueryable<Item> query, string? subject)
        {
            return string.IsNullOrWhiteSpace(subject)
                ? query
                : query.Where(i => i.Book.Subject.Name == subject);
        }

        private IQueryable<Item> ApplyPriceFilters(IQueryable<Item> query, decimal? minPrice, decimal? maxPrice)
        {
            return query.Where(i => !minPrice.HasValue || i.Price >= minPrice.Value)
                        .Where(i => !maxPrice.HasValue || i.Price <= maxPrice.Value);
        }

        private IQueryable<Item> ApplyLevelFilter(IQueryable<Item> query, string? level)
        {
            return string.IsNullOrWhiteSpace(level)
                ? query
                : query.Where(i => i.Book.Level == level.Equals("Rozszerzenie", StringComparison.OrdinalIgnoreCase));
        }
    }
}