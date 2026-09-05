using System.Threading.Tasks;
using Booker.Data;
using Booker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Booker.Areas.Admin.Pages
{
    public class UsersModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SessionCacheManager _sessionCacheManager;
        private readonly ItemManager _itemManager;
        private readonly UserPhotoManager _userPhotoManager;
        private readonly FavoritesManager _favoritesManager;
        private readonly ILogger<UsersModel> _logger;
        private readonly DataContext _context;
        private readonly AdminLockoutOptions _adminLockoutOptions;

        public UsersModel(UserManager<User> userManager, SessionCacheManager sessionCacheManager, ItemManager itemManager, UserPhotoManager userPhotoManager, FavoritesManager favoritesManager, ILogger<UsersModel> logger, DataContext context, IOptions<AdminLockoutOptions> adminLockoutOptions)
        {
            _userManager = userManager;
            _sessionCacheManager = sessionCacheManager;
            _itemManager = itemManager;
            _userPhotoManager = userPhotoManager;
            _favoritesManager = favoritesManager;
            _logger = logger;
            _context = context;
            _adminLockoutOptions = adminLockoutOptions.Value;
        }

        public record LockoutLinkModel(int UserId, string? UserName, bool ShouldLockout);

        public List<User> Users { get; set; } = [];

        [FromQuery]
        public string? SearchTerm { get; set; } = string.Empty;
        public async Task<IActionResult> OnGetAsync()
        {
            Users = await _userManager.Users.ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnGetSearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                Users = await _userManager.Users.ToListAsync();
            }
            else
            {
                Users = await _userManager.Users
                    .Where(u => u.UserName!.Contains(searchTerm) || u.Email!.Contains(searchTerm))
                    .ToListAsync();
            }

            return Partial("_UserRows", Users);
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var deletedUserName = user.UserName ?? id.ToString();

            // The keys must be collected before the account is deleted - the item rows
            // cascade away with the account and the keys cannot be read afterwards.
            var photoKeys = await _userPhotoManager.CollectPhotoKeysAsync(user);

            // RODO - task 09: account deletion and the admin action log entry in a single transaction.
            await using var transaction = await _context.Database.BeginTransactionAsync();

            // Favorites use DeleteBehavior.Restrict, so they must be removed before the user
            // account is deleted or DeleteAsync fails with a foreign key constraint violation.
            await _favoritesManager.RemoveAllFavoritesAsync(user.Id);

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();
                // Handle deletion failure (e.g., log the error, display a message, etc.)
                ModelState.AddModelError(string.Empty, "Error deleting user.");
                Users = _userManager.Users.ToList();
                return new StatusCodeResult(500);
            }

            await _context.LogAdminActionAsync(currentUser, AdminActionTypes.UserDelete, id, deletedUserName, "User");
            await transaction.CommitAsync();

            await _userPhotoManager.DeleteFromStorageAsync(user.Id, photoKeys);

            _logger.LogInformation($"Użytkownik {currentUser?.UserName} usunął konto użytkownika {deletedUserName}.");
            return Content("User deleted successfully.");
        }

        public async Task<IActionResult> OnPostLockoutAsync(int id, int days)
        {
            // Reject anything but the indefinite sentinel (-1) or a bounded positive
            // number of days: empty/invalid input binds as 0 (an immediate, already-expired
            // lockout that still hides the user), and unbounded values could overflow AddDays.
            if (days != -1 && (days < 1 || days > _adminLockoutOptions.MaxDurationDays))
            {
                var message = $"Nieprawidłowa liczba dni blokady. Podaj -1 (bezterminowo) lub liczbę od 1 do {_adminLockoutOptions.MaxDurationDays}.";
                ModelState.AddModelError(string.Empty, message);

                // The lockout dialog's submit button targets its own error box directly
                // (see _UserRows.cshtml), so the message is actually displayed to the admin
                // and the dialog is left open instead of silently closing on failure.
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Content = message,
                    ContentType = "text/plain"
                };
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);

            DateTimeOffset? lockoutEnd;
            if (days < 0)
            {
                lockoutEnd = DateTimeOffset.MaxValue; // Lockout indefinitely
            }
            else
            {
                lockoutEnd = DateTimeOffset.UtcNow.AddDays(days);
            }
            
            _sessionCacheManager.InvalidateSession(id);

            // RODO - task 09: account lockout and the admin action log entry in a single transaction.
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var result = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();
                // Handle lockout failure (e.g., log the error, display a message, etc.)
                ModelState.AddModelError(string.Empty, "Error locking out user.");
                Users = _userManager.Users.ToList();
                return new StatusCodeResult(500);
            }

            user.IsVisible = false;
            var visibilityResult = await _userManager.UpdateAsync(user);
            if (!visibilityResult.Succeeded)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Nie udało się zaktualizować widoczności użytkownika.");
                Users = _userManager.Users.ToList();
                return new StatusCodeResult(500);
            }

            await _itemManager.SetItemsVisibilityByUserAsync(id, false);

            await _context.LogAdminActionAsync(currentUser, AdminActionTypes.UserLockout, user.Id, user.UserName ?? id.ToString(), "User", $"days={days}");
            await transaction.CommitAsync();

            _logger.LogInformation($"Użytkownik {currentUser?.UserName} zablokował konto użytkownika {user.UserName} na okres {days} dni.");
            return Partial("_UserRows", new List<User> { user });
        }

        public async Task<IActionResult> OnPostUnlockAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);

            // RODO - task 09: account unlock and the admin action log entry in a single transaction.
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();
                // Handle unlock failure (e.g., log the error, display a message, etc.)
                ModelState.AddModelError(string.Empty, "Error unlocking user.");
                Users = _userManager.Users.ToList();
                return new StatusCodeResult(500);
            }

            user.IsVisible = true;
            var visibilityResult = await _userManager.UpdateAsync(user);
            if (!visibilityResult.Succeeded)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Nie udało się zaktualizować widoczności użytkownika.");
                Users = _userManager.Users.ToList();
                return new StatusCodeResult(500);
            }

            await _itemManager.SetItemsVisibilityByUserAsync(id, true);

            await _context.LogAdminActionAsync(currentUser, AdminActionTypes.UserUnlock, user.Id, user.UserName ?? id.ToString(), "User");
            await transaction.CommitAsync();

            _logger.LogInformation($"Użytkownik {currentUser?.UserName} odblokował konto użytkownika {user.UserName}.");
            return Partial("_UserRows", new List<User> { user });
        }
    }
}
