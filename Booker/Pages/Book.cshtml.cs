using Booker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using Booker.Services;
using Booker.Utilities;
using SQLitePCL;
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
        private readonly IChatThreadService _chatThreadService;
        private readonly ILogger<BookModel> _logger;


        public Item BookItem { get; set; } = null!;
        public bool IsCurrentUserOwner { get; set; }
        public bool IsFavorite { get; set; } = false;

        public BookModel(UserManager<User> userManager, ItemManager itemManager, FavoritesManager favoritesManager, IAuthorizationService authService, IChatThreadService chatThreadService, ILogger<BookModel> logger)
        {
            _userManager = userManager;
            _itemManager = itemManager;
            _favoritesManager = favoritesManager;
            _authService = authService;
            _chatThreadService = chatThreadService;
            _logger = logger;
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

            var isAuthorized = await _authService.AuthorizeAsync(User, item, ItemOperations.Read);

            if (!item.IsVisible && !isAuthorized.Succeeded)
            {
                _logger.LogWarning($"Użytkownik {User.Identity?.Name} próbował wykonać nieuprawnioną akcję {ItemOperations.Read.Name} na zasobie o ID {id}.");
                return NotFound();
            }

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

        public async Task<IActionResult> OnPostReserveAsync(int id, bool reserve)
        {
            var item = await _itemManager.GetItemAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User).IntOrDefault();

            if (userId == -1 || userId != item.User.Id)
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

        /// <summary>
        /// Starts (or reopens) the conversation about this listing with its seller.
        /// Threads about offers can only be created from here — never user-to-user "cold".
        /// </summary>
        public async Task<IActionResult> OnPostChatAsync(int id, CancellationToken ct)
        {
            var userId = _userManager.GetUserId(User).IntOrDefault();
            if (userId == -1)
            {
                return Challenge();
            }

            try
            {
                var thread = await _chatThreadService.GetOrCreateForItemAsync(userId, id, ct);
                return RedirectToPage("/Chat", new { DealId = thread.ChannelId });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Chat start for item {ItemId} by user {UserId} rejected: {Reason}", id, userId, ex.Message);
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