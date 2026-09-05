using Booker.Data;
using Booker.Services;
using Booker.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Booker.Pages.Profile
{
    /// <summary>
    /// Seller-side sale confirmation: the prompt shown on the seller's own profile
    /// for listings reserved 7+ days ago with no decision yet ("Did you manage to
    /// sell it, and to whom?"). The named buyer is the only user who may rate.
    /// </summary>
    [Authorize]
    public class SaleConfirmationModel(UserManager<User> userManager, ItemManager itemManager, DataContext context) : PageModel
    {
        public List<ItemManager.SalePendingItem> PendingItems { get; private set; } = [];
        public Dictionary<int, List<BuyerOption>> BuyersByItem { get; private set; } = [];

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = userManager.GetUserId(User).IntOrDefault();

            PendingItems = await itemManager.GetSalePendingItemsAsync(userId);
            BuyersByItem = await LoadBuyerOptionsAsync(userId, PendingItems.Select(p => p.Id).ToList());
            return Page();
        }

        public async Task<IActionResult> OnPostConfirmAsync(int itemId, int? soldToUserId)
        {
            var (sellerId, item) = await LoadPendingItemAsync(itemId);
            if (item == null)
            {
                return Forbid();
            }

            // The named buyer must really have chatted about this listing; anything
            // else means "sold outside TextBooker" and earns nobody rating rights.
            var allowed = await LoadBuyerOptionsAsync(sellerId, [itemId]);
            if (soldToUserId.HasValue && !allowed[itemId].Any(b => b.UserId == soldToUserId.Value))
            {
                ModelState.AddModelError(string.Empty, "Wybrany kupujący nie prowadził rozmowy o tym ogłoszeniu.");
                PendingItems = await itemManager.GetSalePendingItemsAsync(sellerId);
                BuyersByItem = await LoadBuyerOptionsAsync(sellerId, PendingItems.Select(p => p.Id).ToList());
                return Page();
            }

            await itemManager.MarkItemSoldAsync(itemId, soldToUserId);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeclineAsync(int itemId)
        {
            var (_, item) = await LoadPendingItemAsync(itemId);
            if (item == null)
            {
                return Forbid();
            }

            await itemManager.MarkItemNotSoldAsync(itemId);
            return RedirectToPage();
        }

        private async Task<(int SellerId, ItemManager.SalePendingItem?)> LoadPendingItemAsync(int itemId)
        {
            var sellerId = userManager.GetUserId(User).IntOrDefault();
            var pending = await itemManager.GetSalePendingItemsAsync(sellerId);
            return (sellerId, pending.FirstOrDefault(p => p.Id == itemId));
        }

        /// <summary>Users the seller conversed with about each pending listing.</summary>
        private async Task<Dictionary<int, List<BuyerOption>>> LoadBuyerOptionsAsync(int sellerId, List<int> itemIds)
        {
            var threads = await context.ChatThreads
                .AsNoTracking()
                .Where(t => t.ItemId != null && itemIds.Contains(t.ItemId.Value))
                .ToListAsync();

            var candidateIds = threads
                .Select(t => t.UserAId == sellerId ? t.UserBId : t.UserAId)
                .Distinct()
                .ToList();
            var names = await context.Users
                .Where(u => candidateIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.UserName ?? $"U{u.Id}");

            return threads
                .GroupBy(t => t.ItemId!.Value)
                .ToDictionary(
                    g => g.Key,
                    g => g
                        .Select(t => t.UserAId == sellerId ? t.UserBId : t.UserAId)
                        .Distinct()
                        .Select(id => new BuyerOption(id, names.GetValueOrDefault(id, "Konto usunięte")))
                        .OrderBy(b => b.DisplayName)
                        .ToList());
        }

        public record BuyerOption(int UserId, string DisplayName);
    }
}
