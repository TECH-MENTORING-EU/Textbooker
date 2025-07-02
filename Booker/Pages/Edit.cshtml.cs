using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Booker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Linq;

namespace Booker.Pages
{
    public class EditModel : PageModel
    {
        private readonly DataContext _context;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;

        public bool IsFirstLoad { get; set; } = false;

        public List<Book> _BooksRaw { get; set; } = new();
        public List<Subject> _SubjectsRaw { get; set; } = new();
        public List<Grade> _GradesRaw { get; set; } = new();

        public List<SelectListItem> Books { get; set; } = new();
        public List<SelectListItem> Subjects { get; set; } = new();
        public List<SelectListItem> Grades { get; set; } = new();
        public List<SelectListItem> Levels { get; set; } = new();

        [BindProperty]
        public InputModel Input { get; set; } = null!;

        public Item? ItemToEdit { get; set; }

        public class InputModel
        {
            public int ItemId { get; set; }

            [Required(ErrorMessage = "Proszê wybraæ tytu³ ksi¹¿ki.")]
            public string Title { get; set; } = string.Empty;
            [Required(ErrorMessage = "Proszê wybraæ przedmiot.")]
            public string Subject { get; set; } = string.Empty;
            [Required(ErrorMessage = "Proszê wybraæ klasê.")]
            public string Grade { get; set; } = string.Empty;
            [Required(ErrorMessage = "Proszê wybraæ poziom.")]
            public string Level { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            [Required(ErrorMessage = "Proszê opisaæ stan ksi¹¿ki.")]
            [StringLength(40, ErrorMessage = "Opis stanu ksi¹¿ki nie mo¿e przekraczaæ 40 znaków.")]
            public string State { get; set; } = string.Empty;
            [Required(ErrorMessage = "Proszê podaæ cenê.")]
            [Range(0.01, double.MaxValue, ErrorMessage = "Cena musi byæ wiêksza od zera.")]
            public decimal Price { get; set; } = 0;

            [Display(Name = "Zdjêcie ksi¹¿ki")]
            public IFormFile? Image { get; set; }

            [BindNever]
            public string? ExistingImageBlobName { get; set; } = string.Empty;
        }

        public EditModel(DataContext context, BlobServiceClient blobServiceClient, IConfiguration config, IMemoryCache cache)
        {
            _context = context;
            _blobServiceClient = blobServiceClient;
            _config = config;
            _cache = cache;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var isUserAuthenticated = User.Identity?.IsAuthenticated ?? false;

            if (!isUserAuthenticated)
            {
                return Redirect("/Identity/Account/Login");
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Redirect("/Identity/Account/Login");
            }

            ItemToEdit = await _context.Items
                .Include(i => i.Book)
                    .ThenInclude(b => b.Subject)
                .Include(i => i.Book)
                    .ThenInclude(b => b.Grades)
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.Id == id && i.User.Id == userId);

            if (ItemToEdit == null)
            {
                return NotFound();
            }

            Input = new InputModel
            {
                ItemId = ItemToEdit.Id,
                Title = ItemToEdit.Book.Title ?? string.Empty,
                Subject = ItemToEdit.Book.Subject.Name ?? string.Empty,
                Grade = ItemToEdit.Book.Grades.Any() ? ItemToEdit.Book.Grades.First().GradeNumber ?? string.Empty : string.Empty,
                Level = ItemToEdit.Book.Level == true ? "Rozszerzenie" : "Podstawa",
                Description = ItemToEdit.Description ?? string.Empty,
                State = ItemToEdit.State ?? string.Empty,
                Price = ItemToEdit.Price,
                ExistingImageBlobName = ItemToEdit.Photo
            };

            await LoadAllDropdownOptions();

            Books.ForEach(b => b.Selected = b.Value == Input.Title);
            Subjects.ForEach(s => s.Selected = s.Value == Input.Subject);
            Grades.ForEach(g => g.Selected = g.Value == Input.Grade);
            Levels.ForEach(l => l.Selected = l.Value == Input.Level);

            return Page();
        }

        public async Task LoadAllDropdownOptions()
        {
            if (!_cache.TryGetValue("allBooks", out List<Book>? allBooks))
            {
                allBooks = await _context.Books
                    .Include(b => b.Grades)
                    .Include(b => b.Subject)
                    .OrderBy(b => b.Id)
                    .ToListAsync();
                _cache.Set("allBooks", allBooks, TimeSpan.FromHours(1));
            }
            _BooksRaw = allBooks!;

            if (!_cache.TryGetValue("allSubjects", out List<Subject>? allSubjects))
            {
                allSubjects = await _context.Subjects
                    .OrderBy(s => s.Name)
                    .ToListAsync();
                _cache.Set("allSubjects", allSubjects, TimeSpan.FromHours(1));
            }
            _SubjectsRaw = allSubjects!;

            if (!_cache.TryGetValue("allGrades", out List<Grade>? allGrades))
            {
                allGrades = await _context.Grades
                    .OrderBy(g => g.Id)
                    .ToListAsync();
                _cache.Set("allGrades", allGrades, TimeSpan.FromHours(1));
            }
            _GradesRaw = allGrades!;

            Books = _BooksRaw
                        .OrderBy(b => b.Title)
                        .Select(b => b.Title)
                        .Distinct()
                        .Select(t => new SelectListItem
                        {
                            Value = t,
                            Text = t
                        }).ToList();

            Subjects = _SubjectsRaw
                        .OrderBy(s => s.Name)
                        .Select(s => new SelectListItem
                        {
                            Value = s.Name,
                            Text = s.Name
                        }).ToList();

            Grades = _GradesRaw
                        .OrderBy(g => g.GradeNumber)
                        .Select(g => new SelectListItem
                        {
                            Value = g.GradeNumber,
                            Text = $"Klasa {g.GradeNumber}"
                        }).ToList();

            var allLevelsRaw = _BooksRaw
                                .Select(b => b.Level)
                                .Distinct()
                                .ToList();

            Levels = allLevelsRaw
                        .Select(l => new SelectListItem
                        {
                            Value = (l == true) ? "Rozszerzenie" : "Podstawa",
                            Text = (l == true) ? "Rozszerzenie" : "Podstawa"
                        })
                        .OrderBy(v => v.Value)
                        .ToList();

            if (!Books.Any(i => string.IsNullOrEmpty(i.Value))) Books.Insert(0, new SelectListItem { Value = "", Text = "Wybierz ksi¹¿kê", Selected = true, Disabled = true });
            if (!Subjects.Any(i => string.IsNullOrEmpty(i.Value))) Subjects.Insert(0, new SelectListItem { Value = "", Text = "Wybierz przedmiot", Selected = true, Disabled = true });
            if (!Grades.Any(i => string.IsNullOrEmpty(i.Value))) Grades.Insert(0, new SelectListItem { Value = "", Text = "Wybierz klasê", Selected = true, Disabled = true });
            if (!Levels.Any(i => string.IsNullOrEmpty(i.Value))) Levels.Insert(0, new SelectListItem { Value = "", Text = "Wybierz poziom", Selected = true, Disabled = true });
        }

        public async Task LoadFilteredDropdownOptions()
        {
            if (!_cache.TryGetValue("allBooks", out List<Book>? allBooks))
            {
                allBooks = await _context.Books
                    .Include(b => b.Grades)
                    .Include(b => b.Subject)
                    .OrderBy(b => b.Id)
                    .ToListAsync();
                _cache.Set("allBooks", allBooks, TimeSpan.FromHours(1));
            }
            _BooksRaw = allBooks!;

            if (!_cache.TryGetValue("allSubjects", out List<Subject>? allSubjects))
            {
                allSubjects = await _context.Subjects
                    .OrderBy(s => s.Name)
                    .ToListAsync();
                _cache.Set("allSubjects", allSubjects, TimeSpan.FromHours(1));
            }
            _SubjectsRaw = allSubjects!;

            if (!_cache.TryGetValue("allGrades", out List<Grade>? allGrades))
            {
                allGrades = await _context.Grades
                    .OrderBy(g => g.Id)
                    .ToListAsync();
                _cache.Set("allGrades", allGrades, TimeSpan.FromHours(1));
            }
            _GradesRaw = allGrades!;

            Books = ApplyFilters(_BooksRaw.AsQueryable())
                         .OrderBy(b => b.Title)
                         .Select(b => b.Title)
                         .Distinct()
                         .Select(t => new SelectListItem
                         {
                             Value = t,
                             Text = t,
                             Selected = t == Input.Title
                         }).ToList();

            if (Books.IsNullOrEmpty())
            {
                Books.Add(new SelectListItem
                {
                    Value = "null",
                    Text = "Brak dostêpnych ksi¹¿ek",
                    Disabled = true
                });
            }

            Grades = FilterGrades(_GradesRaw).Select(g => new SelectListItem
            {
                Value = g.GradeNumber,
                Text = $"Klasa {g.GradeNumber}.",
                Selected = g.GradeNumber == Input.Grade
            }).ToList();

            if (Grades.Count == 1 && string.IsNullOrEmpty(Input.Grade))
            {
                ModelState.Remove("Input.Grade");
                Input.Grade = Grades[0].Value;
            }

            Subjects = FilterSubjects(_SubjectsRaw).Select(s => new SelectListItem
            {
                Value = s.Name,
                Text = s.Name,
                Selected = s.Name == Input.Subject
            }).ToList();
            if (Subjects.Count == 1 && string.IsNullOrEmpty(Input.Subject))
            {
                ModelState.Remove("Input.Subject");
                Input.Subject = Subjects[0].Value;
            }

            var levelsRaw = FilterLevels(_BooksRaw.Select(b => b.Level).Distinct()).ToList();
            Levels = levelsRaw.Select(l => new SelectListItem
            {
                Value = (l == true) ? "Rozszerzenie" : "Podstawa",
                Text = (l == true) ? "Rozszerzenie" : "Podstawa",
                Selected = ((l == true) ? "Rozszerzenie" : "Podstawa") == Input.Level
            }).OrderBy(v => v.Value).ToList();
            if (Levels.Count == 1 && string.IsNullOrEmpty(Input.Level))
            {
                ModelState.Remove("Input.Level");
                Input.Level = Levels[0].Value;
            }
        }

        public async Task<IActionResult> OnGetParamsAsync(bool firstLoad, [ValidateNever] InputModel input)
        {
            if (input != null)
            {
                Input = input;
                if (Input.ItemId > 0)
                {
                    var item = await _context.Items.FindAsync(Input.ItemId);
                    if (item != null)
                    {
                        Input.ExistingImageBlobName = item.Photo;
                    }
                }
            }
            IsFirstLoad = firstLoad;
            await LoadFilteredDropdownOptions();
            return Partial("_FormSelectsEdit", this);
        }

        private IEnumerable<Grade> FilterGrades(IEnumerable<Grade> query)
        {
            return string.IsNullOrWhiteSpace(Input?.Title)
                ? query
                : _BooksRaw.Where(b => b.Title.Equals(Input.Title, StringComparison.OrdinalIgnoreCase)).SelectMany(b => b.Grades).Distinct();
        }

        private IEnumerable<Subject> FilterSubjects(IEnumerable<Subject> query)
        {
            return string.IsNullOrWhiteSpace(Input?.Title)
                ? query
                : _BooksRaw.Where(b => b.Title.Equals(Input.Title, StringComparison.OrdinalIgnoreCase)).Select(b => b.Subject).Distinct();
        }

        private IEnumerable<bool?> FilterLevels(IEnumerable<bool?> query)
        {
            return string.IsNullOrWhiteSpace(Input?.Title)
                ? query
                : _BooksRaw.Where(b => b.Title.Equals(Input.Title, StringComparison.OrdinalIgnoreCase)).Select(b => b.Level).Distinct();
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

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Redirect("/Identity/Account/Login");
            }

            ItemToEdit = await _context.Items
                .Include(i => i.Book)
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.Id == Input.ItemId && i.User.Id == userId);

            if (ItemToEdit == null)
            {
                return NotFound();
            }

            Input.ExistingImageBlobName = ItemToEdit.Photo;

            if (!ModelState.IsValid)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                await LoadAllDropdownOptions();
                Books.ForEach(b => b.Selected = b.Value == Input.Title);
                Subjects.ForEach(s => s.Selected = s.Value == Input.Subject);
                Grades.ForEach(g => g.Selected = g.Value == Input.Grade);
                Levels.ForEach(l => l.Selected = l.Value == Input.Level);
                return Page();
            }

            if (Input.Image != null && Input.Image.Length > 0)
            {
                var containerName = _config["AzureStorage:ContainerName"];
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                if (!string.IsNullOrEmpty(ItemToEdit.Photo))
                {
                    try
                    {
                        var oldBlobName = Path.GetFileName(new Uri(ItemToEdit.Photo).LocalPath);
                        var oldBlobClient = containerClient.GetBlobClient(oldBlobName);
                        await oldBlobClient.DeleteIfExistsAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"B³¹d podczas usuwania starego zdjêcia: {ex.Message}");
                    }
                }

                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Input.Image.FileName);
                var blobClient = containerClient.GetBlobClient(fileName);

                using (var stream = Input.Image.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }
                ItemToEdit.Photo = blobClient.Uri.ToString();
            }
            else if (!string.IsNullOrEmpty(Input.ExistingImageBlobName))
            {
                ItemToEdit.Photo = Input.ExistingImageBlobName;
            }
            else
            {
                ItemToEdit.Photo = string.Empty;
            }

            var book = await _context.Books
                .Include(b => b.Subject)
                .Include(b => b.Grades)
                .Where(b => b.Title.Equals(Input.Title)
                            && b.Subject.Name.Equals(Input.Subject)
                            && b.Grades.Any(g => g.GradeNumber.Equals(Input.Grade))
                            && b.Level == Input.Level.Equals("Rozszerzenie", StringComparison.OrdinalIgnoreCase))
                .FirstOrDefaultAsync();

            if (book == null)
            {
                ModelState.AddModelError(string.Empty, "Nie znaleziono pasuj¹cej ksi¹¿ki. Proszê sprawdziæ wprowadzone dane.");
                Response.StatusCode = StatusCodes.Status400BadRequest;
                await LoadAllDropdownOptions();
                Books.ForEach(b => b.Selected = b.Value == Input.Title);
                Subjects.ForEach(s => s.Selected = s.Value == Input.Subject);
                Grades.ForEach(g => g.Selected = g.Value == Input.Grade);
                Levels.ForEach(l => l.Selected = l.Value == Input.Level);
                return Page();
            }

            ItemToEdit.Book = book!;
            ItemToEdit.Description = Input.Description;
            ItemToEdit.State = Input.State;
            ItemToEdit.Price = Input.Price;
            ItemToEdit.DateTime = DateTime.Now;

            _context.Items.Update(ItemToEdit);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Book", new { id = ItemToEdit.Id });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int itemId)
        {
            var isUserAuthenticated = User.Identity?.IsAuthenticated ?? false;

            if (!isUserAuthenticated)
            {
                return Redirect("/Identity/Account/Login");
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Redirect("/Identity/Account/Login");
            }

            var itemToDelete = await _context.Items
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.User.Id == userId);

            if (itemToDelete == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(itemToDelete.Photo))
            {
                try
                {
                    var containerName = _config["AzureStorage:ContainerName"];
                    var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                    var blobName = Path.GetFileName(new Uri(itemToDelete.Photo).LocalPath);
                    var blobClient = containerClient.GetBlobClient(blobName);
                    await blobClient.DeleteIfExistsAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"B³¹d podczas usuwania zdjêcia z Azure Blob Storage: {ex.Message}");
                }
            }

            _context.Items.Remove(itemToDelete);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Index");
        }
    }
}