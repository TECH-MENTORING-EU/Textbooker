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

    // Name attribute of the select that fired the current Params request
    // (from the HX-Trigger-Name header); empty on the initial firstLoad call.
    public string TriggerName { get; private set; } = string.Empty;

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
        TriggerName = Request.Headers.TryGetValue("HX-Trigger-Name", out var triggerName)
            ? triggerName.ToString()
            : string.Empty;
        await LoadSelects(TriggerName);
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
            if (result.HasFlag(ItemManager.Status.InvalidGrades))
            {
                ModelState.AddModelError("Input.Grade", "Wybrane klasy nie pasują do wybranej książki.");
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

    public async Task LoadSelects(string trigger)
    {
        if (trigger == "Input.Title" && string.IsNullOrWhiteSpace(Input?.Title))
        {
            ModelState.Remove("Input.Title");
            Input!.Title = "";
            ModelState.Remove("Input.Grade");
            Input!.Grade = "";
            ModelState.Remove("Input.Subject");
            Input!.Subject = "";
            ModelState.Remove("Input.Level");
            Input!.Level = "";
        }

        if (trigger == "Input.Subject")
        {
            // A new subject starts a fresh selection. Grade and level left over
            // from the previously added book silently filtered the new subject's
            // titles down to "Brak dostępnych książek" (e.g. picking German after
            // a rozszerzenie math book hid every Welttour Deutsch title).
            ModelState.Remove("Input.Grade");
            Input!.Grade = "";
            ModelState.Remove("Input.Level");
            Input!.Level = "";
        }

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

        // "Inna" is the escape hatch for books missing from the catalog (the form
        // hint points at it). Its subject is the "Brak" pseudo-subject, so the
        // subject filter always hides it - re-add it so it stays reachable for
        // every subject selection.
        if (Books.All(b => b.Value != "Inna"))
        {
            Books.Add(new SelectListItem
            {
                Value = "Inna",
                Text = "Inna"
            });
        }
    }
    
    private async Task LoadGradesSelect()
    {
        var isTitleSet = !string.IsNullOrWhiteSpace(Input?.Title);

        var grades = await (isTitleSet
            ? _staticDataManager.GetGradesByBookTitleAsync(Input!.Title)
            : _staticDataManager.GetGradesAsync());

        if (isTitleSet)
        {
            Grades =
            [
                new SelectListItem
                {
                    Value = string.Join(',',grades.Select(g => g.GradeNumber)),
                    Text = $"Klasa {string.Join(" / ", grades.Select(g => g.GradeNumber))}",
                    Selected = true
                }
            ];
        }
        else
        {
            Grades = grades.Select(g => new SelectListItem
            {
                Value = g.GradeNumber,
                Text = $"Klasa {g.GradeNumber}."
            }).ToList();
        }

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
        var levels = await (string.IsNullOrWhiteSpace(Input?.Title)
            ? _staticDataManager.GetLevelsAsync()
            : _staticDataManager.GetLevelsByBookTitleAsync(Input.Title));

        Levels = levels.Select(l => new SelectListItem
        {
            Value = l.Name,
            Text = l.Name
        }).ToList();

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
    string TriggerName { get; }
}