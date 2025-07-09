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

        const int PageSize = 25;

        public record PagedListViewModel(List<ItemModel> Items, FilterParameters Params, bool HasMorePages);
        public record ItemModel(Item Item, FilterParameters Params, bool IsFavorite, bool IsCurrentUserOwner);
        public record FilterParameters(Grade? Grade, Subject? Subject, bool? Level, int PageNumber);

        public PagedListViewModel? ItemsList { get; set; }
        public List<SelectListItem>? Grades { get; set; }
        public List<SelectListItem>? Subjects { get; set; }
        public List<SelectListItem> Levels => new List<SelectListItem>
        {
            new SelectListItem { Value = "Podstawa", Text = "Podstawa" },
            new SelectListItem { Value = "Rozszerzenie", Text = "Rozszerzenie" }
        };

        public FilterParameters? Params { get; set; }

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

            var convParams = await _staticDataManager.ConvertParametersAsync(
                null,
                Input?.Grade,
                Input?.Subject,
                Input?.Level
            );

            Params = new FilterParameters(
                convParams.Grade,
                convParams.Subject,
                convParams.Level,
                pageNumber
            );

            var params2 = new ItemManager.Parameters(
                Input?.Search,
                convParams.Grade,
                convParams.Subject,
                convParams.Level,
                Input?.MinPrice,
                Input?.MaxPrice
            );

            var totalItems = await _itemManager.GetItemsCountByParamsAsync(params2);
            bool hasMorePages = totalItems > (pageNumber + 1) * PageSize;

            var userId = _userManager.GetUserId(User).IntOrDefault();
            var userFavorites = await _favoritesManager.GetFavoriteIdsAsync(userId);

            var itemsFromDb = await _itemManager.GetPagedItemsByParamsAsync(params2, pageNumber, PageSize);

            var itemModels = itemsFromDb.Select(item => new ItemModel(
                item,
                Params,
                userFavorites.Contains(item.Id),
                item.User.Id == userId
            )).ToList();

            ItemsList = new PagedListViewModel(itemModels, Params, hasMorePages);

            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return Partial("_ItemGallery", ItemsList);
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