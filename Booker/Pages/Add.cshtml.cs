using Azure.Storage.Blobs; // Dodaj to using
using Azure.Storage.Blobs.Models; // Dodaj to using
using Booker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration; // Dodaj to using
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;


namespace Booker.Pages
{
    public class BookAddingModel : PageModel
    {
        private readonly DataContext _context;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;

        public bool IsFirstLoad { get; set; } = false;

        public required List<Book> _Books { get; set; } = new();
        public required List<SelectListItem> Books { get; set; } = new();
        public required List<Subject> _Subjects { get; set; } = new();
        public required List<SelectListItem> Subjects { get; set; } = new();
        public required List<Grade> _Grades { get; set; } = new();
        public required List<SelectListItem> Grades { get; set; } = new();
        public required List<SelectListItem> Levels { get; set; } = new();

        [BindProperty]
        public InputModel Input { get; set; } = null!;

        public class InputModel
        {
            [Required(ErrorMessage = "Proszę wybrać tytuł książki.")]
            public required string Title { get; set; } = string.Empty;
            [Required(ErrorMessage = "Proszę wybrać przedmiot.")]
            public required string Subject { get; set; } = string.Empty;
            [Required(ErrorMessage = "Proszę wybrać klasę.")]
            public required string Grade { get; set; } = string.Empty;
            [Required(ErrorMessage = "Proszę wybrać poziom.")]
            public required string Level { get; set; } = string.Empty;
            public required string Description { get; set; } = string.Empty;
            [Required(ErrorMessage = "Proszę opisać stan książki.")]
            [StringLength(40, ErrorMessage = "Opis stanu książki nie może przekraczać 40 znaków.")]
            public required string State { get; set; } = string.Empty;
            [Required(ErrorMessage = "Proszę podać cenę.")]
            [Range(0.01, double.MaxValue, ErrorMessage = "Cena musi być większa od zera.")]
            public required decimal Price { get; set; } = 0;
            [Required(ErrorMessage = "Proszę przesłać zdjęcie książki.")]
            //[FileExtensions(Extensions = "jpg,jpeg,png,gif", ErrorMessage = "Dozwolone są tylko pliki graficzne (jpg, jpeg, png, gif).")]
            //[Length(0, 5 * 1024 * 1024, ErrorMessage = "Plik nie może przekraczać 5 MB.")]
            [Display(Name = "Zdjęcie książki")]
            public required IFormFile Image { get; set; } = null!;
        }

        public BookAddingModel(DataContext context, BlobServiceClient blobServiceClient, IConfiguration config, IMemoryCache cache)
        {
            _context = context;
            _blobServiceClient = blobServiceClient;
            _config = config;
            _cache = cache;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var isUserAuthenticated = User.Identity?.IsAuthenticated ?? false;

            if (!isUserAuthenticated)
            {
                return Redirect("/Identity/Account/Login");
            }

            await LoadData();
            
            return Page();
        }

        public async Task<IActionResult> OnGetParamsAsync(bool firstLoad, [ValidateNever] InputModel input)
        {
            Input = input;
            IsFirstLoad = firstLoad;
            await LoadData();

            return Partial("_FormSelects", this);
        }

        public async Task LoadData()
        {
            await LoadBooks();
            await LoadGrades();
            await LoadSubjects();
            await LoadLevels();
        }

        private async Task LoadBooks()
        {
            if (!_cache.TryGetValue("books", out List<Book>? books))
            {
                books = await _context.Books
                    .Include(b => b.Grades)
                    .Include(b => b.Subject)
                    .OrderBy(g => g.Id)
                    .ToListAsync();
                _cache.Set("books", books, TimeSpan.FromHours(1));
            }

            _Books = books!;

            Books = ApplyFilters(_Books.AsQueryable())
                   .OrderBy(b => b.Title)
                   .Select(b => b.Title)
                   .Distinct()
                   .Select(t => new SelectListItem
                   {
                       Value = t,
                       Text = t
                   }).ToList();

            if (Books.IsNullOrEmpty())
            {
                Books.Add(new SelectListItem
                {
                    Value = "null",
                    Text = "Brak dostępnych książek",
                    Disabled = true
                });
            }
        }

        private async Task LoadGrades()
        {
            if (!_cache.TryGetValue("grades", out List<Grade>? grades))
            {
                grades = await _context.Grades
                    .OrderBy(g => g.Id)
                    .ToListAsync();
                _cache.Set("grades", grades, TimeSpan.FromHours(1));
            }
            _Grades = grades!;

            Grades = FilterGrades(_Grades).Select(g => new SelectListItem
            {
                Value = g.GradeNumber,
                Text = $"Klasa {g.GradeNumber}."
            }).ToList();

            if (Grades.Count == 1)
            {
                ModelState.Remove("Input.Grade");
                Input.Grade = Grades[0].Value;
            }
        }

        private async Task LoadSubjects()
        {
            if (!_cache.TryGetValue("subjects", out List<Subject>? subjects))
            {
                subjects = await _context.Subjects
                    .OrderBy(s => s.Name)
                    .ToListAsync();
                _cache.Set("subjects", subjects, TimeSpan.FromHours(1));
            }
            _Subjects = subjects!;
            Subjects = FilterSubjects(_Subjects).Select(s => new SelectListItem
            {
                Value = s.Name,
                Text = s.Name
            }).ToList();
            if (Subjects.Count == 1)
            {
                ModelState.Remove("Input.Subject");
                Input.Subject = Subjects[0].Value;
            }
        }

        private async Task LoadLevels()
        {
            var levels = await _context.Books
                .Select(b => b.Level)
                .Distinct()
                .ToListAsync();
            Levels = FilterLevels(levels).Select(l => new SelectListItem
            {
                Value = (l == true) ? "Rozszerzenie" : "Podstawa",
                Text = (l == true) ? "Rozszerzenie" : "Podstawa"
            }).OrderBy(v => v.Value).ToList();
            if (Levels.Count == 1)
            {
                ModelState.Remove("Input.Level");
                Input.Level = Levels[0].Value;
            }
        }

        private IEnumerable<Grade> FilterGrades(IEnumerable<Grade> query)
        {
            return string.IsNullOrWhiteSpace(Input?.Title)
                ? query
                : _Books.Where(b => b.Title.Equals(Input.Title, StringComparison.OrdinalIgnoreCase)).SelectMany(b => b.Grades).Distinct();
        }

        private IEnumerable<Subject> FilterSubjects(IEnumerable<Subject> query)
        {
            return string.IsNullOrWhiteSpace(Input?.Title)
                ? query
                : _Books.Where(b => b.Title.Equals(Input.Title, StringComparison.OrdinalIgnoreCase)).Select(b => b.Subject).Distinct();
        }

        private IEnumerable<bool?> FilterLevels(IEnumerable<bool?> query)
        {
            return string.IsNullOrWhiteSpace(Input?.Title)
                ? query
                : _Books.Where(b => b.Title.Equals(Input.Title, StringComparison.OrdinalIgnoreCase)).Select(b => b.Level).Distinct();
        }

        private IQueryable<Book> ApplyFilters(IQueryable<Book> query)
        {
            query = ApplyTitleFilter(query);
            query = ApplyGradeFilter(query);
            query = ApplySubjectFilter(query);
            query = ApplyLevelFilter(query);

            return query;
        }

        private IQueryable<Book> ApplyTitleFilter(IQueryable<Book> query)
        {
            return string.IsNullOrWhiteSpace(Input?.Title)
                ? query
                : query.Where(b => b.Title.Equals(Input.Title, StringComparison.OrdinalIgnoreCase));
        }

        private IQueryable<Book> ApplyGradeFilter(IQueryable<Book> query)
        {
            return string.IsNullOrWhiteSpace(Input?.Grade)
                ? query
                : query.Where(b => b.Grades.Any(g => g.GradeNumber == Input.Grade));
        }

        private IQueryable<Book> ApplySubjectFilter(IQueryable<Book> query)
        {
            return string.IsNullOrWhiteSpace(Input?.Subject)
                ? query
                : query.Where(b => b.Subject.Name == Input.Subject);
        }

        private IQueryable<Book> ApplyLevelFilter(IQueryable<Book> query)
        {
            return string.IsNullOrWhiteSpace(Input?.Level)
                ? query
                : query.Where(b => b.Level == Input.Level.Equals("Rozszerzenie", StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var isUserAuthenticated = User.Identity?.IsAuthenticated ?? false;

            if (!isUserAuthenticated)
            {
                return Redirect("/Identity/Account/Login");
            }

            if (!ModelState.IsValid)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Page();
            }

            if (Input == null)
            {
                ModelState.AddModelError(string.Empty, "Nieprawidłowe dane wejściowe. Proszę spróbować ponownie.");
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Page();
            }


            if (Input.Image == null || Input.Image.Length == 0)
            {
                ModelState.AddModelError(nameof(Input.Image), "Proszę przesłać zdjęcie książki.");
            }

            if (!ModelState.IsValid)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Page();
            }

            var query = _context.Books
                                     .Include(b => b.Subject)
                                     .Include(b => b.Grades)
                                     .Where(b => b.Title.Equals(Input.Title));

            if (!await query.AnyAsync())
            {
                ModelState.AddModelError(nameof(Input.Title), "Wybrana książka nie została znaleziona w bazie. Proszę wybrać tytuł z listy.");
                await OnGetAsync();
                return Page();
            }

            var querySubject = query.Where(b => b.Subject.Name.Equals(Input.Subject, StringComparison.OrdinalIgnoreCase));

            if (!await querySubject.AnyAsync())
            {
                ModelState.AddModelError(nameof(Input.Subject), "Wybrany przedmiot nie pasuje do wybranej książki.");
            }

            var queryGrades = query.Where(b => b.Grades.Any(g => g.GradeNumber.Equals(Input.Grade, StringComparison.OrdinalIgnoreCase)));

            if (!await queryGrades.AnyAsync())
            {
                ModelState.AddModelError(nameof(Input.Grade), "Wybrana klasa nie pasuje do wybranej książki.");
            }

            bool bookLevel = Input.Level.Equals("Rozszerzenie", StringComparison.OrdinalIgnoreCase);
            var queryLevel = query.Where(b => b.Level == bookLevel);

            if (!await queryLevel.AnyAsync())
            {
                ModelState.AddModelError(nameof(Input.Level), "Wybrany poziom nie pasuje do wybranej książki.");
            }

            var book = await query.Intersect(querySubject).Intersect(queryGrades).Intersect(queryLevel).FirstOrDefaultAsync();

            if (book == null)
            {
                ModelState.AddModelError(string.Empty, "Nie znaleziono pasującej książki. Proszę sprawdzić wprowadzone dane.");
            }

            if (!ModelState.IsValid)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Page();
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return Redirect("/Identity/Account/Login");
            }

            if (!int.TryParse(userIdString, out int userId))
            {
                return Redirect("/Identity/Account/Login");
            }

            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return Redirect("/Identity/Account/Login");
            }

            var item = new Item
            {
                BookId = book!.Id,
                UserId = userId,
                Description = Input.Description,
                State = Input.State,
                Price = Input.Price,
                DateTime = DateTime.Now
            };

            var containerName = _config["AzureStorage:ContainerName"];
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Input.Image.FileName);
            var blobClient = containerClient.GetBlobClient(fileName);

            using (var stream = Input.Image.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }
            item.Photo = blobClient.Uri.ToString();

            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            return Redirect("/Book/" + item.Id);
        }
    }
}