using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Booker.Data;
using Booker.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
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
        private readonly UserManager<User> _userManager;
        private readonly StaticDataManager _staticDataManager;

        public bool IsFirstLoad { get; set; } = false;
        public required List<SelectListItem> Books { get; set; } = new();
        public required List<SelectListItem> Subjects { get; set; } = new();
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

        public BookAddingModel(DataContext context, BlobServiceClient blobServiceClient, IConfiguration config, IMemoryCache cache, UserManager<User> userManager, StaticDataManager staticDataManager)
        {
            _context = context;
            _blobServiceClient = blobServiceClient;
            _config = config;
            _cache = cache;
            _userManager = userManager;
            _staticDataManager = staticDataManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var isUserAuthenticated = User.Identity?.IsAuthenticated ?? false;

            if (!isUserAuthenticated)
            {
                return Redirect("/Identity/Account/Login");
            }

            await LoadSelects();
            
            return Page();
        }

        public async Task<IActionResult> OnGetParamsAsync(bool firstLoad, [ValidateNever] InputModel input)
        {
            Input = input;
            IsFirstLoad = firstLoad;
            await LoadSelects();

            return Partial("_FormSelects", this);
        }

        public async Task LoadSelects()
        {
            await LoadBooksSelect();
            await LoadGradesSelect();
            await LoadSubjectsSelect();
            await LoadLevelsSelect();
        }

        private async Task LoadBooksSelect()
        {
            var books = await _staticDataManager.GetBooksByParamsAsync(
                await _staticDataManager.ConvertParametersAsync(
                    Input?.Title,
                    Input?.Grade,
                    Input?.Subject,
                    Input?.Level
                )
            );

            Books = books
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

        private async Task LoadGradesSelect()
        {
            var grades = await (string.IsNullOrWhiteSpace(Input?.Title)
                ? _staticDataManager.GetGradesAsync()
                : _staticDataManager.GetGradesByBookTitleAsync(Input.Title));

            Grades = grades.Select(g => new SelectListItem
            {
                Value = g.GradeNumber,
                Text = $"Klasa {g.GradeNumber}."
            }).ToList();

            if (Grades.Count == 1)
            {
                ModelState.Remove("Input.Grade");
                Input!.Grade = Grades[0].Value;
            }
        }

        private async Task LoadSubjectsSelect()
        {
            var subjects = await (string.IsNullOrWhiteSpace(Input?.Title)
                ? _staticDataManager.GetSubjectsAsync()
                : _staticDataManager.GetSubjectsByBookTitleAsync(Input.Title));

            Subjects = subjects.Select(s => new SelectListItem
            {
                Value = s.Name,
                Text = s.Name
            }).ToList();

            if (Subjects.Count == 1)
            {
                ModelState.Remove("Input.Subject");
                Input!.Subject = Subjects[0].Value;
            }
        }

        private async Task LoadLevelsSelect()
        {
            var levels = string.IsNullOrWhiteSpace(Input?.Title)
                ? new() { false, true }
                : await _staticDataManager.GetLevelsByBookTitleAsync(Input.Title);

            Levels = levels.Select(l => new SelectListItem
            {
                Value = (l == true) ? "Rozszerzenie" : "Podstawa",
                Text = (l == true) ? "Rozszerzenie" : "Podstawa"
            }).OrderBy(v => v.Value).ToList();

            if (Levels.Count == 1)
            {
                ModelState.Remove("Input.Level");
                Input!.Level = Levels[0].Value;
            }
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

            var querySubject = query.Where(b => b.Subject.Name.Equals(Input.Subject));

            if (!await querySubject.AnyAsync())
            {
                ModelState.AddModelError(nameof(Input.Subject), "Wybrany przedmiot nie pasuje do wybranej książki.");
            }

            var queryGrades = query.Where(b => b.Grades.Any(g => g.GradeNumber.Equals(Input.Grade)));

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

            var book = await _context.Books
                .Include(b => b.Subject)
                .Include(b => b.Grades)
                .Where(b => b.Title.Equals(Input.Title)
                         && b.Subject.Name.Equals(Input.Subject)
                         && b.Grades.Any(g => g.GradeNumber.Equals(Input.Grade))
                         && b.Level == bookLevel)
                .FirstOrDefaultAsync();


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

            var user= await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return Redirect("/Identity/Account/Login");
            }

            var containerName = _config["AzureStorage:ContainerName"];
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Input.Image!.FileName);
            var blobClient = containerClient.GetBlobClient(fileName);

            using (var stream = Input.Image.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            var item = new Item
            {
                Book = book!,
                User = user,
                Description = Input.Description,
                State = Input.State,
                Price = Input.Price,
                DateTime = DateTime.Now,
                Photo = blobClient.Uri.ToString()
            };

            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            return Redirect("/Book/" + item.Id);
        }
    }
}