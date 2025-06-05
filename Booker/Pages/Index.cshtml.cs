using Booker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Booker.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Booker.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly DataContext _context;
        private readonly IMemoryCache _cache;
        const int PageSize = 25;

        public record PagedListViewModel(List<Item> Items, FilterParameters Params, bool HasMorePages);
        public record ItemModel(Item Item, FilterParameters Params);
        public record FilterParameters(Grade? Grade, Subject? Subject, bool? Level, int PageNumber);

        public PagedListViewModel? ItemsList { get; set; }
        public List<SelectListItem>? Grades { get; set; }
        private List<Grade>? _grades;
        public List<SelectListItem>? Subjects { get; set; }
        private List<Subject>? _subjects;
        public List<SelectListItem> Levels => new List<SelectListItem>
        {
            new SelectListItem { Value = "Podstawa", Text = "Podstawa" },
            new SelectListItem { Value = "Rozszerzenie", Text = "Rozszerzenie" }
        };

        public FilterParameters? Params { get; set; }

        public IndexModel(ILogger<IndexModel> logger, DataContext context, IMemoryCache cache)
        {
            _logger = logger;
            _context = context;
            _cache = cache;
        }

        [FromQuery]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public string? Search { get; set; }
            public string? Grade { get; set; }
            public string? Subject { get; set; }
            public decimal? MinPrice { get; set; }
            public decimal? MaxPrice { get; set; }
            public string? Level { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int pageNumber)
        {
            await LoadCache();

            var query = _context.Items
                .Include(i => i.Book).ThenInclude(b => b.Grades)
                .Include(i => i.Book).ThenInclude(b => b.Subject)
                .Include(i => i.User)
                .AsQueryable();

            Params = GetFilterParameters(pageNumber);

            query = ApplyFilters(query);

            var totalItems = await query.CountAsync();
            bool hasMorePages = totalItems > (pageNumber + 1) * PageSize;

            var items = await query
                .OrderBy(i => i.DateTime)
                .Skip(pageNumber * PageSize)
                .Take(PageSize)
                .ToListAsync();
            
            ItemsList = new PagedListViewModel(items, Params, hasMorePages);

            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return Partial("_ItemGallery", ItemsList);
            }
            return Page();
        }

        public async Task<IActionResult> OnGetMoreAsync(int pageNumber)
        {
            var query = _context.Items
                .Include(i => i.Book).ThenInclude(b => b.Grades)
                .Include(i => i.Book).ThenInclude(b => b.Subject)
                .Include(i => i.User)
                .AsQueryable();

            query = ApplyFilters(query);

            var totalItems = await query.CountAsync();
            bool hasMorePages = totalItems > (pageNumber + 1) * PageSize;

            var items = await query
                .OrderBy(i => i.DateTime)
                .Skip(pageNumber * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var pagedListViewModel = new PagedListViewModel(items, GetFilterParameters(pageNumber), hasMorePages);

            return Partial("_ItemGallery", pagedListViewModel);
        }

        private async Task LoadCache()
        {
            if (!_cache.TryGetValue("grades", out List<Grade>? grades))
            {
                grades = await _context.Grades
                    .OrderBy(g => g.Id)
                    .ToListAsync();
                _cache.Set("grades", grades, TimeSpan.FromHours(1));
            }

            _grades = grades;

            Grades = _grades?.Select(g => new SelectListItem
            {
                Value = g.GradeNumber,
                Text = g.GradeNumber
            }).ToList();

            if (!_cache.TryGetValue("subjects", out List<Subject>? subjects))
            {
                subjects = await _context.Subjects
                    .OrderBy(s => s.Name)
                    .ToListAsync();
                _cache.Set("subjects", subjects, TimeSpan.FromHours(1));
            }

            _subjects = subjects;

            Subjects = _subjects?.Select(s => new SelectListItem
            {
                Value = s.Name,
                Text = s.Name
            }).ToList();
        }

        private FilterParameters GetFilterParameters(int pageNumber)
        {
            return new FilterParameters
            (
                string.IsNullOrWhiteSpace(Input.Grade) ? null : _grades?.FirstOrDefault(g => g.GradeNumber == Input.Grade),
                string.IsNullOrWhiteSpace(Input.Subject) ? null : _subjects?.FirstOrDefault(s => s.Name == Input.Subject),
                string.IsNullOrWhiteSpace(Input.Level) ? null : Input.Level.Equals("Rozszerzenie", StringComparison.OrdinalIgnoreCase),
                pageNumber
            );
        }

        private IQueryable<Item> ApplyFilters(IQueryable<Item> query)
        {
            query = ApplySearchFilter(query, Input.Search);
            query = ApplyGradeFilter(query, Input.Grade);
            query = ApplySubjectFilter(query, Input.Subject);
            query = ApplyPriceFilters(query, Input.MinPrice, Input.MaxPrice);
            query = ApplyLevelFilter(query, Input.Level);

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