using Booker.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using System.Security.Claims;
using Booker.Services;
using Booker.Areas.Identity.Utilities;
using Booker.Utilities;


namespace Booker.Pages
{
    public class BookModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ItemManager _itemManager;
        private readonly FavoritesManager _favoritesManager;

        public Item BookItem { get; set; } = null!;
        public bool IsCurrentUserOwner { get; set; }
        public bool IsFavorite { get; set; } = false;

        public BookModel(UserManager<User> userManager, ItemManager itemManager, FavoritesManager favoritesManager)
        {
            _userManager = userManager;
            _itemManager = itemManager;
            _favoritesManager = favoritesManager;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var item = await _itemManager.GetItemAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            BookItem = item;

            var userId = _userManager.GetUserId(User).IntOrDefault();

            IsFavorite = await _favoritesManager.IsFavoriteAsync(userId, id);

            IsCurrentUserOwner = userId == BookItem.User.Id;

            return Page();
        }

        public async Task<IActionResult> OnGetEmailAsync(int id)
        {
            var item = await _itemManager.GetItemAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            BookItem = item;
            
            var userId = _userManager.GetUserId(User).IntOrDefault();

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