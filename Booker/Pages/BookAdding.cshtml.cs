using Booker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace Booker.Pages
{
    public class BookAddingModel : PageModel
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _environment;

        public List<string>? Subjects { get; set; }
        public List<string>? Grades { get; set; }
        public List<string>? Books { get; set; }
        public bool IsUserAuthenticated { get; set; }

        [BindProperty]
        public string Title { get; set; } = string.Empty;

        [BindProperty]
        public string Subject { get; set; } = string.Empty;

        [BindProperty]
        public string Grade { get; set; } = string.Empty;

        [BindProperty]
        public string Level { get; set; } = string.Empty;

        [BindProperty]
        public string Description { get; set; } = string.Empty;

        [BindProperty]
        public string State { get; set; } = string.Empty;

        [BindProperty]
        public decimal Price { get; set; }

        [BindProperty]
        public IFormFile BookImage { get; set; }

        public BookAddingModel(DataContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            IsUserAuthenticated = User.Identity?.IsAuthenticated ?? false;

            if (IsUserAuthenticated)
            {
                Subjects = await _context.Subjects.OrderBy(s => s.Name).Select(s => s.Name).ToListAsync();
                Grades = await _context.Grades.OrderBy(g => g.Id).Select(g => g.GradeNumber).ToListAsync();
                Books = await _context.Books.OrderBy(b => b.Title).Select(b => b.Title).ToListAsync();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            IsUserAuthenticated = User.Identity?.IsAuthenticated ?? false;

            if (!IsUserAuthenticated)
            {
                ModelState.AddModelError(string.Empty, "Musisz siê zalogowaæ, aby dodaæ og³oszenie.");
                await OnGetAsync();
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Subject) || string.IsNullOrWhiteSpace(Grade))
            {
                ModelState.AddModelError(string.Empty, "Wype³nij wszystkie wymagane pola.");
                await OnGetAsync();
                return Page();
            }

            var book = await _context.Books.FirstOrDefaultAsync(b => b.Title == Title);
            if (book == null)
            {
                ModelState.AddModelError(string.Empty, "Nie znaleziono ksi¹¿ki w bazie.");
                await OnGetAsync();
                return Page();
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                ModelState.AddModelError(string.Empty, "Nie znaleziono identyfikatora u¿ytkownika.");
                await OnGetAsync();
                return Page();
            }

            if (!int.TryParse(userIdString, out int userId))
            {
                ModelState.AddModelError(string.Empty, "Nieprawid³owy identyfikator u¿ytkownika.");
                await OnGetAsync();
                return Page();
            }

            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                ModelState.AddModelError(string.Empty, "U¿ytkownik nie istnieje.");
                await OnGetAsync();
                return Page();
            }

            var item = new Item
            {
                BookId = book.Id,
                UserId = userId,
                Description = Description,
                State = State,
                Price = Price,
                DateTime = DateTime.Now
            };

            if (BookImage != null && BookImage.Length > 0)
            {
                var fileName = Path.GetFileName(BookImage.FileName);
                var filePath = Path.Combine(_environment.WebRootPath, "img", "books", fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await BookImage.CopyToAsync(fileStream);
                }

                item.Photo = "/img/books/" + fileName;
            }
            else
            {
                item.Photo = "/img/books/default.jpg";
            }

            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
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

            var books = await query.OrderBy(b => b.Title).Select(b => b.Title).ToListAsync();

            return Content(string.Join("", books.Select(b => $"<option value='{b}'>{b}</option>")), "text/html");
        }
    }
}