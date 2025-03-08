using Booker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Booker.Pages
{
    public class BookAddingModel : PageModel
    {
        private readonly DataContext _context;

        public List<string>? Subjects { get; set; }
        public List<string>? Grades { get; set; }
        public List<string>? Books { get; set; }

        public BookAddingModel(DataContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            Subjects = await _context.Subjects.OrderBy(s => s.Name).Select(s => s.Name).ToListAsync();
            Grades = await _context.Grades.OrderBy(g => g.Id).Select(g => g.GradeNumber).ToListAsync();
            Books = await _context.Books.OrderBy(b => b.Title).Select(b => b.Title).ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnGetBooksByFiltersAsync(string? subject, string? grade, string? level)
        {
            var query = _context.Books.AsQueryable();

            if (!string.IsNullOrWhiteSpace(subject))
            {
                query = query.Where(b => b.Subject.Name == subject);
            }

            if (!string.IsNullOrWhiteSpace(grade))
            {
                query = query.Where(b => b.Grades.Any(g => g.GradeNumber == grade));
            }

            if (!string.IsNullOrWhiteSpace(level))
            {
                bool isExtended = level.Equals("Rozszerzony", StringComparison.OrdinalIgnoreCase);
                query = query.Where(b => b.Level == isExtended);
            }

            var books = await query
                .OrderBy(b => b.Title)
                .Select(b => b.Title)
                .ToListAsync();

            return Content(string.Join("", books.Select(b => $"<option value='{b}'>{b}</option>")), "text/html");
        }
    }
}
