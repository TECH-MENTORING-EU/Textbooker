using Booker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Booker.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Booker.Services;
using Booker.Utilities;
using Microsoft.AspNetCore.Identity;

namespace Booker.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ItemManager _itemManager;
        private readonly FavoritesManager _favoritesManager;
        private readonly StaticDataManager _staticDataManager;
        private readonly UserManager<User> _userManager;

        public List<int> ItemIds { get; set; } = null!;
        public StaticDataManager.Parameters Params { get; set; } = null!;
        public List<SelectListItem>? Grades { get; set; }
        public List<SelectListItem>? Subjects { get; set; }
        public List<SelectListItem> Levels => new List<SelectListItem>
        {
            new SelectListItem { Value = "Podstawa", Text = "Podstawa" },
            new SelectListItem { Value = "Rozszerzenie", Text = "Rozszerzenie" }
        };

        public IndexModel(
            ILogger<IndexModel> logger,
            ItemManager itemManager,
            FavoritesManager favoritesManager,
            StaticDataManager staticDataManager,
            UserManager<User> userManager
            )
        {
            _logger = logger;
            _itemManager = itemManager;
            _favoritesManager = favoritesManager;
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
            public decimal? MinPrice { get; set; }
            public decimal? MaxPrice { get; set; }
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
                Params.Grade,
                Params.Subject,
                Params.Level,
                Input?.MinPrice,
                Input?.MaxPrice
            );

            ItemIds = (await _itemManager.GetItemsByParamsAsync(params2)).Select(i => i.Id).ToList();       

            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return ViewComponent("ItemGalleryViewComponent", new
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
        }
    }
}