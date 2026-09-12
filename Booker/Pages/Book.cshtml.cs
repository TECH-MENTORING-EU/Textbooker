using Booker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using Booker.Services;
using Booker.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Booker.Authorization;

namespace Booker.Pages
{
    public class BookModel(
        UserManager<User> userManager,
        ItemManager itemManager,
        FavoritesManager favoritesManager,
        IAuthorizationService authService,
        IChatThreadService chatThreadService,
        IRatingManager ratingManager,
        ILogger<BookModel> logger,
        ContactRevealLimiter contactRevealLimiter,
        IConfiguration configuration) : PageModel
    {
        public List<string> Photos { get; set; } = new();

        public Item BookItem { get; set; } = null!;
        public bool IsCurrentUserOwner { get; set; }
        public bool IsFavorite { get; set; } = false;
        public int ViewCount { get; set; }
        public bool CanRateSeller { get; set; }

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

            IsCurrentUserOwner = currentUser != null && currentUser.Id == BookItem.User.Id;
            IsFavorite = currentUser != null && await favoritesManager.IsFavoriteAsync(currentUser.Id, id);

            var isAuthorized = await authService.AuthorizeAsync(User, item, ItemOperations.Read);

            if (!item.IsVisible && !isAuthorized.Succeeded)
            {
                logger.LogWarning("Użytkownik {UserName} próbował wykonać nieuprawnioną akcję {ActionName} na zasobie o ID {ItemId}.",
                    User.Identity?.Name, ItemOperations.Read.Name, id);
                return NotFound();
            }

            if (currentUser != null && !IsCurrentUserOwner)
            {
                await itemManager.TrackViewAsync(id, currentUser.Id);
            }

            if (IsCurrentUserOwner)
            {
                ViewCount = await itemManager.GetViewCountAsync(id);
            }
            else if (currentUser != null)
            {
                CanRateSeller = await ratingManager.CanRateAsync(currentUser.Id, BookItem.UserId);
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

            // RODO - task 05: the seller's contact details are only disclosed in the context of
            // an active, publicly visible listing (basis: contract performance).
            var isAuthorized = await authService.AuthorizeAsync(User, item, ItemOperations.Read);
            if (!item.IsVisible && !isAuthorized.Succeeded)
            {
                return NotFound();
            }

            if (currentUser == null)
            {
                Response.Headers["HX-Redirect"] = Url.Page("/Account/Login", new { area = "Identity" });
                return new NoContentResult();
            }

            if (BookItem.User.Id == currentUser.Id)
            {
                return new NoContentResult();
            }

            // RODO - task 07: contact-reveal limit - generous, but finite, counted per account
            // in process memory (see ContactRevealLimiter).
            if (!contactRevealLimiter.TryRegisterReveal(currentUser.Id))
            {
                if (contactRevealLimiter.ShouldLogRejection(currentUser.Id))
                {
                    logger.LogWarning(
                        "Użytkownik {UserName} przekroczył limit ujawnień danych kontaktowych.",
                        User.Identity?.Name);
                }

                return Content(
                    "<p role=\"alert\">Zbyt wiele wyświetlonych kontaktów w krótkim czasie. " +
                    "Spróbuj ponownie później albo napisz do nas: " +
                    "<a href=\"mailto:support@textbooker.pl\">support@textbooker.pl</a>.</p>",
                    "text/html");
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

            if (currentUser == null || currentUser.Id != item.User.Id)
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

        /// <summary>
        /// Messages can be dark-launched off: the flag hides the chat button,
        /// and this handler answers 404 so the URL is not usable by
        /// hand-crafted requests either (mirrors ChatModel.MessagesDisabled).
        /// </summary>
        private bool MessagesDisabled => !configuration.GetValue<bool>("Features:MessagesEnabled");

        /// <summary>
        /// Starts (or reopens) the conversation about this listing with its seller.
        /// Threads about offers can only be created from here, never user-to-user "cold".
        /// </summary>
        public async Task<IActionResult> OnPostChatAsync(int id, CancellationToken ct)
        {
            if (MessagesDisabled)
            {
                return NotFound();
            }

            var userId = userManager.GetUserId(User).IntOrDefault();
            if (userId == -1)
            {
                return Challenge();
            }

            try
            {
                var thread = await chatThreadService.GetOrCreateForItemAsync(userId, id, ct);
                return RedirectToPage("/Chat", new { DealId = thread.ChannelId });
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning("Chat start for item {ItemId} by user {UserId} rejected: {Reason}", id, userId, ex.Message);
                return BadRequest();
            }
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