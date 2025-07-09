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
using Microsoft.AspNetCore.Identity;
using Booker.Services;

namespace Booker.Pages
{
    public class EditModel : PageModel
    {
        private readonly DataContext _context;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        private readonly UserManager<User> _userManager;
        private readonly ItemManager _itemManager;
        private readonly StaticDataManager _staticDataManager;
        public bool IsFirstLoad { get; set; } = false;

        public List<Book> _BooksRaw { get; set; } = new();
        public List<Subject> _SubjectsRaw { get; set; } = new();
        public List<Grade> _GradesRaw { get; set; } = new();

        public List<SelectListItem> Books { get; set; } = new();
        public List<SelectListItem> Subjects { get; set; } = new();
        public List<SelectListItem> Grades { get; set; } = new();
        public List<SelectListItem> Levels { get; set; } = new();

        [BindProperty]
        public Shared.ItemEditModel Input { get; set; } = null!;

        public Item? ItemToEdit { get; set; }

        public EditModel(DataContext context, BlobServiceClient blobServiceClient, IConfiguration config, IMemoryCache cache, UserManager<User> userManager, ItemManager itemManager, StaticDataManager staticDataManager)
        {
            _context = context;
            _blobServiceClient = blobServiceClient;
            _config = config;
            _cache = cache;
            _userManager = userManager;
            _itemManager = itemManager;
            _staticDataManager = staticDataManager;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Redirect("/Identity/Account/Login");
            }

            ItemToEdit = await _itemManager.GetItemAsync(id);

            if (ItemToEdit == null)
            {
                return NotFound();
            }

            if (ItemToEdit.User.Id != user.Id)
            {
                return Forbid();
            }

            Input = new Shared.ItemEditModel
            {
                Title = ItemToEdit.Book.Title,
                Subject = ItemToEdit.Book.Subject.Name,
                Grade = ItemToEdit.Book.Grades.First().GradeNumber,
                Level = ItemToEdit.Book.Level == true ? "Rozszerzenie" : "Podstawa",
                Description = ItemToEdit.Description,
                State = ItemToEdit.State,
                Price = ItemToEdit.Price,
                Image = null!
            };

            await LoadSelects();

            return Page();
        }

        public async Task LoadSelects()
        {
            await LoadBooksSelect();
            await LoadGradesSelect();
            await LoadSubjectsSelect();
            await LoadLevelsSelect();

            Books.ForEach(b => b.Selected = b.Value == Input.Title);
            Subjects.ForEach(s => s.Selected = s.Value == Input.Subject);
            Grades.ForEach(g => g.Selected = g.Value == Input.Grade);
            Levels.ForEach(l => l.Selected = l.Value == Input.Level);
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

        public async Task<IActionResult> OnGetParamsAsync(bool firstLoad, [ValidateNever] Shared.ItemEditModel input)
        {
            Input = input;

            IsFirstLoad = firstLoad;
            await LoadSelects();
            return Partial("_FormSelects", this);
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Redirect("/Identity/Account/Login");
            }

            if (Input == null)
            {
                ModelState.AddModelError(string.Empty, "Nieprawidłowe dane wejściowe. Proszę spróbować ponownie.");
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Page();
            }

            if (!ModelState.IsValid)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Page();
            }            

            ItemToEdit = await _itemManager.GetItemAsync(id);

            if (ItemToEdit == null)
            {
                return NotFound();
            }

            if (ItemToEdit.User.Id != user.Id)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Page();
            }
            
            var parameters = await _staticDataManager.ConvertParametersAsync(
                Input.Title,
                Input.Grade,
                Input.Subject,
                Input.Level
            );

            var result = await _itemManager.UpdateItemAsync(ItemToEdit.Id, new ItemManager.ItemModel(
                user,
                parameters,
                Input.Description,
                Input.State,
                Input.Price,
                Input.Image?.OpenReadStream(),
                Path.GetExtension(Input.Image?.FileName),
                ItemToEdit.Photo
            ));

            if (result.HasFlag(ItemManager.Status.Error))
            {
                if (result.HasFlag(ItemManager.Status.InvalidTitle))
                {
                    ModelState.AddModelError("Input.Title", "Wybrana książka nie została znaleziona w bazie. Proszę wybrać tytuł z listy.");
                }
                if (result.HasFlag(ItemManager.Status.InvalidSubject))
                {
                    ModelState.AddModelError("Input.Subject", "Wybrany przedmiot nie pasuje do wybranej książki.");
                }
                if (result.HasFlag(ItemManager.Status.InvalidGrade))
                {
                    ModelState.AddModelError("Input.Grade", "Wybrana klasa nie pasuje do wybranej książki.");
                }
                if (result.HasFlag(ItemManager.Status.InvalidLevel))
                {
                    ModelState.AddModelError("Input.Level", "Wybrany poziom nie pasuje do wybranej książki.");
                }
                if (result.HasFlag(ItemManager.Status.NotFound))
                {
                    ModelState.AddModelError(string.Empty, "Nie znaleziono pasującej książki. Proszę sprawdzić wprowadzone dane.");
                }

                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Page();
            }

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
                    Console.WriteLine($"Błąd podczas usuwania zdjęcia z Azure Blob Storage: {ex.Message}");
                }
            }

            _context.Items.Remove(itemToDelete);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Index");
        }
    }
}