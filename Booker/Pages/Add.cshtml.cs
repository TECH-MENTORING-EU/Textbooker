using Booker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Azure.Storage.Blobs; // Dodaj to using
using Azure.Storage.Blobs.Models; // Dodaj to using
using Microsoft.Extensions.Configuration; // Dodaj to using


namespace Booker.Pages
{
    public class BookAddingModel : PageModel
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _environment; // Mo¿e byæ usuniête, jeœli nie u¿ywasz ju¿ lokalnego przechowywania
        private readonly BlobServiceClient _blobServiceClient; // Nowa zale¿noœæ
        private readonly IConfiguration _config; // Nowa zale¿noœæ


        public bool IsUserAuthenticated { get; set; }

        public List<string>? AllBookTitles { get; set; }
        public List<string>? AllSubjects { get; set; }
        public List<string>? AllGrades { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Proszê wybraæ tytu³ ksi¹¿ki.")]
        public string Title { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Przedmiot jest wymagany.")]
        public string Subject { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Klasa jest wymagana.")]
        public string Grade { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Poziom jest wymagany.")]
        public string Level { get; set; } = string.Empty;

        [BindProperty]
        public string Description { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Proszê opisaæ stan ksi¹¿ki.")]
        [StringLength(40, ErrorMessage = "Opis stanu ksi¹¿ki nie mo¿e przekraczaæ 40 znaków.")]
        public string State { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Proszê podaæ cenê.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Cena musi byæ wiêksza od zera.")]
        public decimal Price { get; set; }

        [BindProperty]
        public IFormFile? BookImage { get; set; }

        public BookAddingModel(DataContext context, IWebHostEnvironment environment, BlobServiceClient blobServiceClient, IConfiguration config)
        {
            _context = context;
            _environment = environment;
            _blobServiceClient = blobServiceClient;
            _config = config;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            IsUserAuthenticated = User.Identity?.IsAuthenticated ?? false;

            if (IsUserAuthenticated)
            {
                AllBookTitles = await _context.Books
                                              .OrderBy(b => b.Title)
                                              .Select(b => b.Title)
                                              .Distinct()
                                              .ToListAsync();
                AllSubjects = new List<string>();
                AllGrades = new List<string>();
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

            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var book = await _context.Books
                                     .Include(b => b.Subject)
                                     .Include(b => b.Grades)
                                     .FirstOrDefaultAsync(b => b.Title.ToLower() == Title.ToLower());

            if (book == null)
            {
                ModelState.AddModelError(nameof(Title), "Wybrana ksi¹¿ka nie zosta³a znaleziona w bazie. Proszê wybraæ tytu³ z listy.");
                await OnGetAsync();
                return Page();
            }

            if (book.Subject?.Name.ToLower() != Subject.ToLower())
            {
                ModelState.AddModelError(nameof(Subject), "Wybrany przedmiot nie pasuje do wybranej ksi¹¿ki.");
                await OnGetAsync(); return Page();
            }
            if (!book.Grades.Any(g => g.GradeNumber.ToLower() == Grade.ToLower()))
            {
                ModelState.AddModelError(nameof(Grade), "Wybrana klasa nie pasuje do wybranej ksi¹¿ki.");
                await OnGetAsync(); return Page();
            }
            string bookLevelString = (book.Level ?? false) ? "Rozszerzony" : "Podstawowy";
            if (bookLevelString.ToLower() != Level.ToLower())
            {
                ModelState.AddModelError(nameof(Level), "Wybrany poziom nie pasuje do wybranej ksi¹¿ki.");
                await OnGetAsync(); return Page();
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                ModelState.AddModelError(string.Empty, "Nie znaleziono identyfikatora u¿ytkownika. Spróbuj siê przelogowaæ.");
                await OnGetAsync();
                return Page();
            }

            if (!int.TryParse(userIdString, out int userId))
            {
                ModelState.AddModelError(string.Empty, "Nieprawid³owy format identyfikatora u¿ytkownika.");
                await OnGetAsync();
                return Page();
            }

            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                ModelState.AddModelError(string.Empty, "U¿ytkownik nie istnieje w bazie danych. Spróbuj siê przelogowaæ.");
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
                var containerName = _config["AzureStorage:ContainerName"];
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(BookImage.FileName);
                var blobClient = containerClient.GetBlobClient(fileName);

                using (var stream = BookImage.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }

                item.Photo = blobClient.Uri.ToString();
            }
            else
            {
                item.Photo = "https://your_account_name.blob.core.windows.net/bookimages/default.jpg";
            }

            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }

        public IActionResult OnPostValidateBookImage()
        {
            if (BookImage == null || BookImage.Length == 0)
            {
                ModelState.AddModelError(nameof(BookImage), "Proszê przes³aæ zdjêcie ksi¹¿ki.");
            }
            return Content(GetValidationMessageForProperty(nameof(BookImage)), "text/html");
        }

        public async Task<IActionResult> OnPostValidateTitle()
        {
            await TryUpdateModelAsync(this, nameof(Title));

            if (ModelState.GetValidationState(nameof(Title)) == ModelValidationState.Valid)
            {
                if (!string.IsNullOrWhiteSpace(Title))
                {
                    var bookExists = await _context.Books.AnyAsync(b => b.Title.ToLower() == Title.ToLower());
                    if (!bookExists)
                    {
                        ModelState.AddModelError(nameof(Title), "Wybrana ksi¹¿ka nie zosta³a znaleziona w bazie. Proszê wybraæ tytu³ z listy.");
                    }
                }
            }
            return Content(GetValidationMessageForProperty(nameof(Title)), "text/html");
        }

        public async Task<IActionResult> OnPostValidateSubject([FromForm] string title)
        {
            await TryUpdateModelAsync(this, nameof(Subject));

            if (ModelState.GetValidationState(nameof(Subject)) == ModelValidationState.Valid)
            {
                if (!string.IsNullOrWhiteSpace(title))
                {
                    var book = await _context.Books
                                             .Include(b => b.Subject)
                                             .FirstOrDefaultAsync(b => b.Title.ToLower() == title.ToLower());
                    if (book == null || book.Subject?.Name.ToLower() != Subject.ToLower())
                    {
                        ModelState.AddModelError(nameof(Subject), "Wybrany przedmiot nie pasuje do wybranej ksi¹¿ki.");
                    }
                }
            }
            return Content(GetValidationMessageForProperty(nameof(Subject)), "text/html");
        }

        public async Task<IActionResult> OnPostValidateGrade([FromForm] string title, [FromForm] string subject)
        {
            await TryUpdateModelAsync(this, nameof(Grade));

            if (ModelState.GetValidationState(nameof(Grade)) == ModelValidationState.Valid)
            {
                if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(subject))
                {
                    var book = await _context.Books
                                             .Include(b => b.Grades)
                                             .Include(b => b.Subject)
                                             .FirstOrDefaultAsync(b => b.Title.ToLower() == title.ToLower() && b.Subject != null && b.Subject.Name.ToLower() == subject.ToLower());
                    if (book == null || !book.Grades.Any(g => g.GradeNumber.ToLower() == Grade.ToLower()))
                    {
                        ModelState.AddModelError(nameof(Grade), "Wybrana klasa nie pasuje do wybranej ksi¹¿ki.");
                    }
                }
            }
            return Content(GetValidationMessageForProperty(nameof(Grade)), "text/html");
        }

        public async Task<IActionResult> OnPostValidateLevel([FromForm] string title, [FromForm] string subject, [FromForm] string grade)
        {
            await TryUpdateModelAsync(this, nameof(Level));

            if (ModelState.GetValidationState(nameof(Level)) == ModelValidationState.Valid)
            {
                if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(subject) && !string.IsNullOrWhiteSpace(grade))
                {
                    var book = await _context.Books
                                             .Include(b => b.Subject)
                                             .Include(b => b.Grades)
                                             .FirstOrDefaultAsync(b => b.Title.ToLower() == title.ToLower()
                                                                          && b.Subject != null && b.Subject.Name.ToLower() == subject.ToLower()
                                                                          && b.Grades.Any(g => g.GradeNumber.ToLower() == grade.ToLower()));
                    string bookLevelString = (book?.Level ?? false) ? "Rozszerzony" : "Podstawowy";
                    if (book == null || bookLevelString.ToLower() != Level.ToLower())
                    {
                        ModelState.AddModelError(nameof(Level), "Wybrany poziom nie pasuje do wybranej ksi¹¿ki.");
                    }
                }
            }
            return Content(GetValidationMessageForProperty(nameof(Level)), "text/html");
        }

        public async Task<IActionResult> OnPostValidateState()
        {
            await TryUpdateModelAsync(this, nameof(State));
            return Content(GetValidationMessageForProperty(nameof(State)), "text/html");
        }

        public async Task<IActionResult> OnPostValidatePrice()
        {
            await TryUpdateModelAsync(this, nameof(Price));
            return Content(GetValidationMessageForProperty(nameof(Price)), "text/html");
        }

        public async Task<IActionResult> OnPostValidateAll()
        {
            await TryUpdateModelAsync(this, "");

            if (BookImage == null || BookImage.Length == 0)
            {
                ModelState.AddModelError(nameof(BookImage), "Proszê przes³aæ zdjêcie ksi¹¿ki.");
            }

            if (ModelState.GetValidationState(nameof(Title)) == ModelValidationState.Valid)
            {
                var book = await _context.Books
                                         .Include(b => b.Subject)
                                         .Include(b => b.Grades)
                                         .FirstOrDefaultAsync(b => b.Title.ToLower() == Title.ToLower());

                if (book == null)
                {
                    ModelState.AddModelError(nameof(Title), "Wybrana ksi¹¿ka nie zosta³a znaleziona w bazie. Proszê wybraæ tytu³ z listy.");
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(Subject))
                    {
                        ModelState.AddModelError(nameof(Subject), "Przedmiot jest wymagany.");
                    }
                    else if (ModelState.GetValidationState(nameof(Subject)) == ModelValidationState.Valid && book.Subject?.Name.ToLower() != Subject.ToLower())
                    {
                        ModelState.AddModelError(nameof(Subject), "Wybrany przedmiot nie pasuje do wybranej ksi¹¿ki.");
                    }

                    if (string.IsNullOrWhiteSpace(Grade))
                    {
                        ModelState.AddModelError(nameof(Grade), "Klasa jest wymagana.");
                    }
                    else if (ModelState.GetValidationState(nameof(Grade)) == ModelValidationState.Valid && !book.Grades.Any(g => g.GradeNumber.ToLower() == Grade.ToLower()))
                    {
                        ModelState.AddModelError(nameof(Grade), "Wybrana klasa nie pasuje do wybranej ksi¹¿ki.");
                    }

                    if (string.IsNullOrWhiteSpace(Level))
                    {
                        ModelState.AddModelError(nameof(Level), "Poziom jest wymagany.");
                    }
                    else if (ModelState.GetValidationState(nameof(Level)) == ModelValidationState.Valid)
                    {
                        string bookLevelString = (book.Level ?? false) ? "Rozszerzony" : "Podstawowy";
                        if (bookLevelString.ToLower() != Level.ToLower())
                        {
                            ModelState.AddModelError(nameof(Level), "Wybrany poziom nie pasuje do wybranej ksi¹¿ki.");
                        }
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                var errorHtml = new System.Text.StringBuilder();
                errorHtml.Append("<div id='formValidationSummary' class='text-danger'>");
                foreach (var modelStateEntry in ModelState.Values)
                {
                    foreach (var error in modelStateEntry.Errors)
                    {
                        errorHtml.Append($"<p>{error.ErrorMessage}</p>");
                    }
                }
                errorHtml.Append("</div>");

                Response.StatusCode = 400;
                return Content(errorHtml.ToString(), "text/html");
            }

            return Content("", "text/html");
        }

        private string GetValidationMessageForProperty(string propertyName)
        {
            var stringBuilder = new System.Text.StringBuilder();
            stringBuilder.Append($"<span id='{propertyName.ToLower()}Error' class='text-danger'>");
            if (ModelState.TryGetValue(propertyName, out var modelStateEntry) && modelStateEntry.Errors.Any())
            {
                stringBuilder.Append(modelStateEntry.Errors.First().ErrorMessage);
            }
            stringBuilder.Append("</span>");
            return stringBuilder.ToString();
        }
    }
}