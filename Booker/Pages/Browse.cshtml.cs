using Booker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Booker.Services;
using Microsoft.AspNetCore.Identity;

namespace Booker.Pages
{
    public class BrowseModel : PageModel
    {
        private readonly ItemManager _itemManager;
        private readonly StaticDataManager _staticDataManager;
        private readonly UserManager<User> _userManager;

        public List<int> ItemIds { get; set; } = new();
        public StaticDataManager.Parameters Params { get; set; } =
            new(null, new List<Grade>(), null, null);
        public List<SelectListItem>? Grades { get; set; }
        public List<SelectListItem>? Subjects { get; set; }
        public List<SelectListItem>? Levels { get; set; }

        public BrowseModel(
            ItemManager itemManager,
            StaticDataManager staticDataManager,
            UserManager<User> userManager
            )
        {
            _itemManager = itemManager;
            _staticDataManager = staticDataManager;
            _userManager = userManager;
        }

        [FromQuery]
        public InputModel? Input { get; set; }
        public class InputModel
        {
            public string? Search { get; set; }
            public string? Grade { get; set; }
            public string? Subject { get; set; }

            // Prices bind as strings: Polish users type the decimal comma
            // ("10,50"), but decimal model binding only accepts the dot, so a
            // comma value would silently bind to null and drop the filter.
            public string? MinPrice { get; set; }
            public string? MaxPrice { get; set; }

            public decimal? MinPriceValue => ParseFlexibleDecimal(MinPrice);
            public decimal? MaxPriceValue => ParseFlexibleDecimal(MaxPrice);

            private static decimal? ParseFlexibleDecimal(string? raw)
            {
                if (string.IsNullOrWhiteSpace(raw)) return null;

                var normalized = raw.Trim().Replace(',', '.');
                if (decimal.TryParse(normalized, System.Globalization.CultureInfo.InvariantCulture,
                    out var dotDecimal))
                {
                    return dotDecimal;
                }

                // Values that keep the Polish thousands separator ("1.234,56")
                // only parse with pl-PL number styles; anything else is dropped.
                return decimal.TryParse(raw.Trim(), System.Globalization.NumberStyles.Number,
                    _polish, out var polishNumber) ? polishNumber : null;
            }

            private static readonly System.Globalization.CultureInfo _polish =
                new("pl-PL");

            public string? Level { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int pageNumber)
        {
            await LoadSelects();

            Params = await _staticDataManager.ConvertParametersAsync(
                null,
                Input?.Grade,
                Input?.Subject,
                Input?.Level
            );

            var params2 = new ItemManager.Parameters(
                Input?.Search,
                Params.Grades,
                Params.Subject,
                Params.Level,
                Input?.MinPriceValue,
                Input?.MaxPriceValue
            );

            var currentUser = User.Identity?.IsAuthenticated == true
                ? await _userManager.GetUserAsync(User)
                : null;

            ItemIds = await _itemManager.GetItemIdsByParamsAsync(params2, currentUser).ToListAsync();

            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return ViewComponent("ItemGallery", new
                {
                    itemIds = ItemIds,
                    parameters = Params,
                    pageNumber = pageNumber
                });
            }
            return Page();
        }

        private async Task LoadSelects()
        {
            var _grades = await _staticDataManager.GetGradesAsync();
            var _subjects = await _staticDataManager.GetSubjectsAsync();
            var _levels = await _staticDataManager.GetLevelsAsync();

            Grades = _grades?.Select(g => new SelectListItem
            {
                Value = g.GradeNumber,
                Text = $"Klasa {g.GradeNumber}."
            }).ToList();

            Subjects = _subjects?.Select(s => new SelectListItem
            {
                Value = s.Name,
                Text = s.Name
            }).ToList();

            Levels = _levels?.Select(l => new SelectListItem
            {
                Value = l.Name,
                Text = l.Name
            }).ToList();
        }
    }
}
