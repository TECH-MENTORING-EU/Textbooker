using System.Threading.Tasks;
using Booker.Data;
using Booker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Booker.Areas.Admin.Pages
{
    public class UsersModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SessionCacheManager _sessionCacheManager;
        private readonly ItemManager _itemManager;
        private readonly UserPhotoManager _userPhotoManager;
        private readonly ILogger<UsersModel> _logger;

        public UsersModel(UserManager<User> userManager, SessionCacheManager sessionCacheManager, ItemManager itemManager, UserPhotoManager userPhotoManager, ILogger<UsersModel> logger)
        {
            _userManager = userManager;
            _sessionCacheManager = sessionCacheManager;
            _itemManager = itemManager;
            _userPhotoManager = userPhotoManager;
            _logger = logger;
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
            if (currentUser == null)
            {
                return NotFound();
            }

            if (user.Id == currentUser.Id)
            {
                ModelState.AddModelError(string.Empty, "Nie możesz usunąć własnego konta z panelu administratora.");
                return new BadRequestResult();
            }

            // The admin's password arrives in the HX-Prompt header, same as the
            // lockout duration; deleting an account must not be weaker than
            // granting a role.
            var password = Request.Headers["HX-Prompt"].ToString();
            if (string.IsNullOrWhiteSpace(password) || !await _userManager.CheckPasswordAsync(currentUser, password))
            {
                ModelState.AddModelError(string.Empty, "Niepoprawne hasło.");
                _logger.LogWarning(
                    "Użytkownik {AdminUserName} próbował usunąć konto użytkownika {TargetUserName}, ale wpisał błędne hasło.",
                    currentUser.UserName, user.UserName);
                return new BadRequestResult();
            }

            // The keys must be collected before the account is deleted - the item rows
            // cascade away with the account and the keys cannot be read afterwards.
            var photoKeys = await _userPhotoManager.CollectPhotoKeysAsync(user);

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                // Handle deletion failure (e.g., log the error, display a message, etc.)
                ModelState.AddModelError(string.Empty, "Error deleting user.");
                Users = _userManager.Users.ToList();
                return new StatusCodeResult(500);
            }

            await _sessionCacheManager.InvalidateSessionAsync(id);
            await _userPhotoManager.DeleteFromStorageAsync(user.Id, photoKeys);

            _logger.LogInformation("Użytkownik {AdminUserName} usunął konto użytkownika {TargetUserName}.",
                currentUser.UserName, user.UserName);
            return Content("User deleted successfully.");
        }

        public async Task<IActionResult> OnPostLockoutAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);

            var daysStr = Request.Headers["HX-Prompt"].ToString();
            if (!int.TryParse(daysStr, out int days))
            {
                ModelState.AddModelError(string.Empty, "Invalid number of days.");
                Users = _userManager.Users.ToList();
                return new BadRequestResult();
            }

            DateTimeOffset? lockoutEnd;
            if (days < 0)
            {
                lockoutEnd = DateTimeOffset.MaxValue; // Lockout indefinitely
            }
            else
            {
                lockoutEnd = DateTimeOffset.UtcNow.AddDays(days);
            }
            
            await _sessionCacheManager.InvalidateSessionAsync(id);
            var result = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
            if (!result.Succeeded)
            {
                // Handle lockout failure (e.g., log the error, display a message, etc.)
                ModelState.AddModelError(string.Empty, "Error locking out user.");
                Users = _userManager.Users.ToList();
                return new StatusCodeResult(500);
            }
            
            user.IsVisible = false;
            await _userManager.UpdateAsync(user);

            await _itemManager.SetItemsVisibilityByUserAsync(id, false);

            _logger.LogInformation("Użytkownik {AdminUserName} zablokował konto użytkownika {TargetUserName} na okres {Days} dni.",
                currentUser?.UserName, user.UserName, days);
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

            var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
            if (!result.Succeeded)
            {
                // Handle unlock failure (e.g., log the error, display a message, etc.)
                ModelState.AddModelError(string.Empty, "Error unlocking user.");
                Users = _userManager.Users.ToList();
                return new StatusCodeResult(500);
            }

            // Drop the invalid entry left by the lockout so the user can sign in
            // again immediately instead of waiting for the next cleanup pass.
            _sessionCacheManager.RemoveCachedSession(id);

            user.IsVisible = true;
            await _userManager.UpdateAsync(user);
            await _itemManager.SetItemsVisibilityByUserAsync(id, true);

            _logger.LogInformation("Użytkownik {AdminUserName} odblokował konto użytkownika {TargetUserName}.",
                currentUser?.UserName, user.UserName);
            return Partial("_UserRows", new List<User> { user });
        }
    }
}
