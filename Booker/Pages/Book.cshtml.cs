using Booker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Booker.Pages
{
    public class BookModel : PageModel
    {
        private readonly DataContext _context;

        public Item? BookItem { get; set; }

        public BookModel(DataContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            BookItem = await _context.Items
                .Include(i => i.Book).ThenInclude(b => b.Grades)
                .Include(i => i.Book).ThenInclude(b => b.Subject)
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (BookItem == null)
            {
                return NotFound();
            }

            return Page();
        }

        public static string FormatDateWithSpecialCases(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return "Brak daty";

            var now = DateTime.Now;
            var date = dateTime.Value;

            if (date.Date == now.Date)
                return $"dzisiaj o {date:HH:mm}";
            if (date.Date == now.Date.AddDays(-1))
                return $"wczoraj o {date:HH:mm}";

            return date.ToString("d MMMM yyyy 'o' HH:mm", new CultureInfo("pl-PL"));
        }
    }
}
