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
        private readonly UserManager<User> _userManager;
        private readonly StaticDataManager _staticDataManager;
        private readonly ItemManager _itemManager;


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

        public BookAddingModel(UserManager<User> userManager, StaticDataManager staticDataManager, ItemManager itemManager)
        {
            _userManager = userManager;
            _staticDataManager = staticDataManager;
            _itemManager = itemManager;
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

            if (Input.Image == null || Input.Image.Length == 0)
            {
                ModelState.AddModelError("Input.Image", "Proszę przesłać zdjęcie książki.");
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

            var result = await _itemManager.AddItemAsync(new ItemManager.ItemModel(
                user,
                parameters,
                Input.Description,
                Input.State,
                Input.Price,
                Input.Image!.OpenReadStream(),
                Path.GetExtension(Input.Image.FileName)
            ));

            if (result.Status.HasFlag(ItemManager.Status.Error))
            {
                if (result.Status.HasFlag(ItemManager.Status.InvalidTitle))
                {
                    ModelState.AddModelError("Input.Title", "Wybrana książka nie została znaleziona w bazie. Proszę wybrać tytuł z listy.");
                }
                if (result.Status.HasFlag(ItemManager.Status.InvalidSubject))
                {
                    ModelState.AddModelError("Input.Subject", "Wybrany przedmiot nie pasuje do wybranej książki.");
                }
                if (result.Status.HasFlag(ItemManager.Status.InvalidGrade))
                {
                    ModelState.AddModelError("Input.Grade", "Wybrana klasa nie pasuje do wybranej książki.");
                }
                if (result.Status.HasFlag(ItemManager.Status.InvalidLevel))
                {
                    ModelState.AddModelError("Input.Level", "Wybrany poziom nie pasuje do wybranej książki.");
                }
                if (result.Status.HasFlag(ItemManager.Status.NotFound))
                {
                    ModelState.AddModelError(string.Empty, "Nie znaleziono pasującej książki. Proszę sprawdzić wprowadzone dane.");
                }

                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Page();
            }

            return Redirect("/Book/" + result.ItemId);
        }
    }
}