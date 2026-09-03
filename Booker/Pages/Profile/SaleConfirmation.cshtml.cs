using Booker.Services;
using Booker.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Booker.Pages.Profile
{
    /// <summary>
    /// Seller-side sale confirmation: the prompt shown on the seller's own profile
    /// for listings reserved 7+ days ago with no decision yet ("Did you manage to sell it?").
    /// </summary>
    [Authorize]
    public class SaleConfirmationModel : PageModel
    {
        private readonly ItemManager _itemManager;
        private readonly UserManager<Data.User> _userManager;

        public SaleConfirmationModel(ItemManager itemManager, UserManager<Data.User> userManager)
        {
            _itemManager = itemManager;
            _userManager = userManager;
        }

        public List<ItemManager.SalePendingItem> PendingItems { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User).IntOrDefault();
            if (userId == -1)
            {
                return Redirect("/Identity/Account/Login");
            }

            PendingItems = await _itemManager.GetSalePendingItemsAsync(userId);
            return Page();
        }

        public async Task<IActionResult> OnPostConfirmAsync(int itemId)
        {
            return await DecideAsync(itemId, sold: true);
        }

        public async Task<IActionResult> OnPostDeclineAsync(int itemId)
        {
            return await DecideAsync(itemId, sold: false);
        }

        private async Task<IActionResult> DecideAsync(int itemId, bool sold)
        {
            var userId = _userManager.GetUserId(User).IntOrDefault();
            var item = await _itemManager.GetItemAsync(itemId);

            // Only the seller may decide, and only while a decision is still pending.
            if (item == null || userId == -1 || item.UserId != userId || item.IsSold || item.ReservedAt == null)
            {
                return Forbid();
            }

            if (sold)
            {
                await _itemManager.MarkItemSoldAsync(itemId);
            }
            else
            {
                await _itemManager.MarkItemNotSoldAsync(itemId);
            }

            return RedirectToPage();
        }
    }
}
