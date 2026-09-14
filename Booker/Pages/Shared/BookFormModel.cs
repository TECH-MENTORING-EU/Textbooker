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

[Flags]
public enum SelectToSwap
{
    None = 0,
    Title = 1,
    Subject = 2,
    Grade = 4,
    Level = 8
}

public abstract class BookFormModel<T> : PageModel, IBookForm where T : ItemInputModel
{
    protected readonly UserManager<User> _userManager;
    protected readonly StaticDataManager _staticDataManager;
    protected readonly ItemManager _itemManager;
    public bool IsFirstLoad { get; set; } = false;
    public SelectToSwap SelectsToSwap { get; private set; } = SelectToSwap.None;

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
        SelectsToSwap = GetSelectsToSwap(TriggerName, firstLoad);
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
            // "Wybierz książkę" starts over. The subject only ever narrowed the book
            // list, so it is cleared with the rest and every title becomes reachable.
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
            ModelState.Remove("Input.Title");
            Input!.Title = "";
            ModelState.Remove("Input.Grade");
            Input!.Grade = "";
            ModelState.Remove("Input.Level");
            Input!.Level = "";
        }

        // Subjects run first so a title-derived subject (see LoadSubjectsSelect) is
        // already set by the time the other three selects get filtered by it.
        await LoadSubjectsSelect();
        await LoadBooksSelect();
        await LoadGradesSelect();
        await LoadLevelsSelect();

        Books.ForEach(b => b.Selected = b.Value == Input?.Title);
        Subjects.ForEach(s => s.Selected = s.Value == Input?.Subject);
        Grades.ForEach(g => g.Selected = g.Value == Input?.Grade);
        Levels.ForEach(l => l.Selected = l.Value == Input?.Level);
    }

    private async Task LoadBooksSelect()
    {
        // No select is filtered by its own value, so the title is left out here.
        // Grade and level go with it once a title is picked: both are then only
        // echoes of that title, and filtering by them would collapse the list to
        // the one book already selected instead of the whole subject.
        var isTitleSet = !string.IsNullOrWhiteSpace(Input?.Title);

        var books = await _staticDataManager.GetBooksByParamsAsync(
            await _staticDataManager.ConvertParametersAsync(
                null,
                isTitleSet ? null : Input?.Grade,
                Input?.Subject,
                isTitleSet ? null : Input?.Level
            )
        );

        Books = books
                .OrderBy(b => b.Title == StaticDataManager.OtherBookTitle)
                .ThenBy(b => b.Title)
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

        // "Inna" is the escape hatch for books missing from the catalog (the form hint
        // points at it). Its row has level "Brak", so picking any other level filters it
        // out - re-add it so it stays reachable.
        if (Books.All(b => b.Value != StaticDataManager.OtherBookTitle))
        {
            Books.Add(new SelectListItem
            {
                Value = StaticDataManager.OtherBookTitle,
                Text = StaticDataManager.OtherBookTitle
            });
        }

        // Same as the grade and level selects: a single option is picked for the user.
        // This matters for a subject whose only book is "Inna" (e.g. "Brak" on STZN) -
        // without a title the grade and level selects would have nothing to offer.
        // A dead end ("Brak dostępnych książek" + "Inna") has two entries and is skipped.
        if (!isTitleSet && Books.Count == 1)
        {
            ModelState.Remove("Input.Title");
            Input!.Title = Books[0].Value;
        }
    }

    private async Task LoadGradesSelect()
    {
        Grades = string.IsNullOrWhiteSpace(Input?.Title)
            ? await BuildGradeOptionsForSubject()
            : await BuildGradeOptionsForTitle(Input.Title);

        if (Grades.Count == 1)
        {
            ModelState.Remove("Input.Grade");
            Input!.Grade = Grades[0].Value;
        }
    }

    // A book spans a fixed set of grades, so the select collapses to that one span.
    private async Task<List<SelectListItem>> BuildGradeOptionsForTitle(string title)
    {
        var gradeNumbers = (await _staticDataManager.GetGradesByBookTitleAsync(title))
            .Select(g => g.GradeNumber)
            .ToList();

        return
        [
            new SelectListItem
            {
                Value = string.Join(',', gradeNumbers),
                Text = $"Klasa {string.Join(" / ", gradeNumbers)}",
                Selected = true
            }
        ];
    }

    // Only grades that some book of the selected subject is actually taught in.
    // Offering the rest would let the user filter the book list down to nothing.
    private async Task<List<SelectListItem>> BuildGradeOptionsForSubject()
    {
        var grades = await _staticDataManager.GetGradesByParamsAsync(
            await _staticDataManager.ConvertParametersAsync(null, null, Input?.Subject, Input?.Level)
        );

        return grades.Select(g => new SelectListItem
        {
            Value = g.GradeNumber,
            Text = $"Klasa {g.GradeNumber}."
        }).ToList();
    }

    private async Task LoadSubjectsSelect()
    {
        var subjects = await _staticDataManager.GetSubjectsAsync();

        Subjects = subjects.Select(s => new SelectListItem
        {
            Value = s.Name,
            Text = s.Name
        }).ToList();

        // A picked title decides the subject, overwriting whatever was selected before -
        // otherwise a subject left over from an earlier pick keeps filtering the book
        // list to a different subject than the title that is now selected. "Inna" has a
        // row in every subject, so it never resolves to a single one and the subject the
        // user picked stays.
        if (string.IsNullOrWhiteSpace(Input?.Title))
            return;

        var bookSubjects = await _staticDataManager.GetSubjectsByBookTitleAsync(Input.Title);
        if (bookSubjects.Count != 1)
            return;

        ModelState.Remove("Input.Subject");
        Input.Subject = bookSubjects[0].Name;
    }

    private async Task LoadLevelsSelect()
    {
        var levels = string.IsNullOrWhiteSpace(Input?.Title)
            ? await GetLevelsForSubject()
            : await _staticDataManager.GetLevelsByBookTitleAsync(Input.Title);

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

    // Only the levels the selected subject is actually published at - every German book
    // is "Podstawa+Rozszerzenie", so offering "Podstawa" would empty the book list.
    private async Task<List<Level>> GetLevelsForSubject() =>
        await _staticDataManager.GetLevelsByParamsAsync(
            await _staticDataManager.ConvertParametersAsync(null, Input?.Grade, Input?.Subject, null)
        );

    private static SelectToSwap GetSelectsToSwap(string triggerName, bool firstLoad)
    {
        if (firstLoad)
            return SelectToSwap.Title | SelectToSwap.Subject | SelectToSwap.Grade | SelectToSwap.Level;

        return triggerName switch
        {
            "Input.Subject" => SelectToSwap.Title | SelectToSwap.Grade | SelectToSwap.Level,
            // Title is swapped too: a picked title may set the subject, and the title list
            // has to be rebuilt for it (LoadBooksSelect never filters by the title itself).
            "Input.Title" => SelectToSwap.Title | SelectToSwap.Subject | SelectToSwap.Grade | SelectToSwap.Level,
            // Grade/Level can narrow each other's list down to a single, auto-selected
            // value (see LoadGradesSelect/LoadLevelsSelect), so both must be swapped
            // alongside Title or the browser keeps a stale dependent value.
            "Input.Grade" or "Input.Level" => SelectToSwap.Title | SelectToSwap.Grade | SelectToSwap.Level,
            _ => SelectToSwap.None
        };
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
    SelectToSwap SelectsToSwap { get; }
}