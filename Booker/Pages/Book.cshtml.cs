using Booker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using Booker.Services;
using Booker.Utilities;
using Microsoft.AspNetCore.Authorization;
using Booker.Authorization;


namespace Booker.Pages
{
    public class BookModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ItemManager _itemManager;
        private readonly FavoritesManager _favoritesManager;
        private readonly IAuthorizationService _authService;
        private readonly ILogger<BookModel> _logger;


        public Item BookItem { get; set; } = null!;
        public bool IsCurrentUserOwner { get; set; }
        public bool IsFavorite { get; set; } = false;
        public int ViewCount { get; set; }

        public BookModel(UserManager<User> userManager, ItemManager itemManager, FavoritesManager favoritesManager, IAuthorizationService authService, ILogger<BookModel> logger)
        {
            _userManager = userManager;
            _itemManager = itemManager;
            _favoritesManager = favoritesManager;
            _authService = authService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var item = await _itemManager.GetItemAsync(id, currentUser);
            if (item == null)
            {
                return NotFound();
            }

            BookItem = item;

            IsCurrentUserOwner = currentUser != null && currentUser.Id == BookItem.User.Id;
            IsFavorite = currentUser != null && await _favoritesManager.IsFavoriteAsync(currentUser.Id, id);

            var isAuthorized = await _authService.AuthorizeAsync(User, item, ItemOperations.Read);

            if (!item.IsVisible && !isAuthorized.Succeeded)
            {
                _logger.LogWarning("Użytkownik {UserName} próbował wykonać nieuprawnioną akcję {ActionName} na zasobie o ID {ItemId}.",
                    User.Identity?.Name, ItemOperations.Read.Name, id);
                return NotFound();
            }

            if (currentUser != null && !IsCurrentUserOwner)
            {
                await _itemManager.TrackViewAsync(id, currentUser.Id);
            }

            if (IsCurrentUserOwner)
            {
                ViewCount = await _itemManager.GetViewCountAsync(id);
            }

            return Page();
        }

        public async Task<IActionResult> OnGetEmailAsync(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var item = await _itemManager.GetItemAsync(id, currentUser);

            if (item == null)
            {
                return NotFound();
            }

            BookItem = item;

            if (currentUser == null)
            {
                Response.Headers["HX-Redirect"] = Url.Page("/Account/Login", new { area = "Identity" });
                return new NoContentResult();
            }

            if (BookItem.User.Id == currentUser.Id)
            {
                return new NoContentResult();
            }

            return Partial("_ContactDetails", BookItem.User);
        }

        public async Task<IActionResult> OnPostReserveAsync(int id, bool reserve)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var item = await _itemManager.GetItemAsync(id, currentUser);
            if (item == null)
            {
                return NotFound();
            }

            if (currentUser == null || currentUser.Id != item.User.Id)
            {
                return Forbid();
            }

            if (item.Reserved != reserve)
            {
                await _itemManager.MarkItemReservedAsync(id, reserve);
            }

            Response.Headers["HX-Refresh"] = "true";
            return new NoContentResult();
        }

        public static string FormatDateWithSpecialCases(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return "Brak daty";

            var now = DateTime.Now;
            var date = dateTime.Value;

            if (date.Date == now.Date)
                return $"dzisiaj o {date:HH:mm}";
            if (date.Date == now.Date.AddDays(-1))
                return $"wczoraj o {date:HH:mm}";

            return date.ToString("d MMMM 'o' HH:mm", new CultureInfo("pl-PL"));
        }
    }
}