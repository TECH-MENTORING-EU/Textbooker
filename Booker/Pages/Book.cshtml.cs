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
    public class BookModel(UserManager<User> userManager, ItemManager itemManager, FavoritesManager favoritesManager, IAuthorizationService authService, ILogger<BookModel> logger) : PageModel
    {
        public List<string> Photos { get; set; } = new();

        public Item BookItem { get; set; } = null!;
        public bool IsCurrentUserOwner { get; set; }
        public bool IsFavorite { get; set; } = false;
        public int ViewCount { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var currentUser = await userManager.GetUserAsync(User);
            var item = await itemManager.GetItemAsync(id, currentUser);
            if (item == null)
            {
                return NotFound();
            }

            Photos = itemManager.GetPhotosUrl(item);
            BookItem = item;

            var userId = userManager.GetUserId(User).IntOrDefault();

            IsFavorite = await favoritesManager.IsFavoriteAsync(userId, id);

            IsCurrentUserOwner = userId == BookItem.User.Id;

            var isAuthorized = await authService.AuthorizeAsync(User, item, ItemOperations.Read);

            if (!item.IsVisible && !isAuthorized.Succeeded)
            {
                logger.LogWarning($"Użytkownik {User.Identity?.Name} próbował wykonać nieuprawnioną akcję {ItemOperations.Read.Name} na zasobie o ID {id}.");
                return NotFound();
            }

            if (userId != -1 && !IsCurrentUserOwner)
            {
                await _itemManager.RecordViewAsync(id, userId);
            }

            if (IsCurrentUserOwner)
            {
                ViewCount = await _itemManager.GetViewCountAsync(id);
            }

            return Page();
        }

        public async Task<IActionResult> OnGetEmailAsync(int id)
        {
            var currentUser = await userManager.GetUserAsync(User);
            var item = await itemManager.GetItemAsync(id, currentUser);

            if (item == null)
            {
                return NotFound();
            }

            BookItem = item;
            var userId = userManager.GetUserId(User).IntOrDefault();

            if (userId == -1)
            {
                Response.Headers["HX-Redirect"] = Url.Page("/Account/Login", new { area = "Identity" });
                return new NoContentResult();
            }

            if (BookItem.User.Id == userId)
            {
                return new NoContentResult();
            }

            return Partial("_ContactDetails", BookItem.User);
        }

        public async Task<IActionResult> OnPostReserveAsync(int id, bool reserve)
        {
            var currentUser = await userManager.GetUserAsync(User);
            var item = await itemManager.GetItemAsync(id, currentUser);
            if (item == null)
            {
                return NotFound();
            }

            var userId = userManager.GetUserId(User).IntOrDefault();

            if (userId == -1 || userId != item.User.Id)
            {
                return Forbid();
            }

            if (item.Reserved != reserve)
            {
                await itemManager.MarkItemReservedAsync(id, reserve);
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