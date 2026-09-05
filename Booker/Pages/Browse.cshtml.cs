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

                // Polish notation: decimal comma ("10,50"), thousands dot or
                // space ("1.234", "1 234"), sometimes both ("1.234,56").
                var s = raw.Trim().Replace(" ", "");

                string normalized;
                if (s.Contains(','))
                {
                    // A comma is always the decimal separator, so any dot can
                    // only group thousands ("1.234,56" -> "1234.56").
                    normalized = s.Replace(".", "").Replace(',', '.');
                }
                else if (LooksLikeDotGrouping(s))
                {
                    // Dot-only with full 3-digit groups is grouping ("1.234" is
                    // 1234, not 1.234); anything else keeps the dot as decimal.
                    normalized = s.Replace(".", "");
                }
                else
                {
                    normalized = s;
                }

                return decimal.TryParse(normalized, System.Globalization.CultureInfo.InvariantCulture,
                    out var value) ? value : null;
            }

            private static bool LooksLikeDotGrouping(string s)
            {
                // "1.234" or "12.345.678": the first group has 1-3 digits and
                // every following group has exactly three.
                var groups = s.Split('.');
                return groups.Length > 1
                    && groups[0].Length is > 0 and <= 3
                    && groups.Skip(1).All(g => g.Length == 3);
            }

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
