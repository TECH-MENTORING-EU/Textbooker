using System;
using Booker.Data;
using Booker.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;

namespace Booker.Pages.Shared;

public abstract class BookFormModel<T> : PageModel, IBookForm where T : ItemInputModel
{
    protected readonly UserManager<User> _userManager;
    protected readonly StaticDataManager _staticDataManager;
    protected readonly ItemManager _itemManager;
    public bool IsFirstLoad { get; set; } = false;

    [BindProperty]
    public T? Input { get; set; }
    ItemInputModel? IBookForm.Input => Input;


    public required List<SelectListItem> Books { get; set; } = new();
    public required List<SelectListItem> Subjects { get; set; } = new();
    public required List<SelectListItem> Grades { get; set; } = new();
    public required List<SelectListItem> Levels { get; set; } = new();

    public BookFormModel(UserManager<User> userManager, StaticDataManager staticDataManager, ItemManager itemManager)
    {
        _userManager = userManager;
        _staticDataManager = staticDataManager;
        _itemManager = itemManager;
    }

    public async Task<IActionResult> OnGetParamsAsync(bool firstLoad, [ValidateNever] T input)
    {
        Input = input;

        IsFirstLoad = firstLoad;
        await LoadSelects();
        return Partial("_FormSelects", this);
    }

    public IActionResult ValidateAndReturn(int itemId, ItemManager.Status result)
    {
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

        return RedirectToPage("/Book", new { id = itemId});
    }

    public async Task LoadSelects()
    {
        await LoadBooksSelect();
        await LoadGradesSelect();
        await LoadSubjectsSelect();
        await LoadLevelsSelect();

        Books.ForEach(b => b.Selected = b.Value == Input?.Title);
        Subjects.ForEach(s => s.Selected = s.Value == Input?.Subject);
        Grades.ForEach(g => g.Selected = g.Value == Input?.Grade);
        Levels.ForEach(l => l.Selected = l.Value == Input?.Level);
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
}

public interface IBookForm
{
    ItemInputModel? Input { get; }
    List<SelectListItem> Books { get; }
    List<SelectListItem> Subjects { get; }
    List<SelectListItem> Grades { get; }
    List<SelectListItem> Levels { get; }
    bool IsFirstLoad { get; }
}