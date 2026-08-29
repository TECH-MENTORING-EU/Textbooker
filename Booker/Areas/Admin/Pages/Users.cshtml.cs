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
        private readonly DataContext _context;

        public UsersModel(UserManager<User> userManager, SessionCacheManager sessionCacheManager, ItemManager itemManager, UserPhotoManager userPhotoManager, ILogger<UsersModel> logger, DataContext context)
        {
            _userManager = userManager;
            _sessionCacheManager = sessionCacheManager;
            _itemManager = itemManager;
            _userPhotoManager = userPhotoManager;
            _logger = logger;
            _context = context;
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

            // RODO — zadanie 09: usunięcie konta i wpis w dzienniku administracyjnym w jednej transakcji.
            await using var transaction = await _context.Database.BeginTransactionAsync();

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
            
            _sessionCacheManager.InvalidateSession(id);

            // RODO — zadanie 09: blokada konta i wpis w dzienniku administracyjnym w jednej transakcji.
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
            await _userManager.UpdateAsync(user);

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

            // RODO — zadanie 09: odblokowanie konta i wpis w dzienniku administracyjnym w jednej transakcji.
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
            await _userManager.UpdateAsync(user);
            await _itemManager.SetItemsVisibilityByUserAsync(id, true);

            await _context.LogAdminActionAsync(currentUser, AdminActionTypes.UserUnlock, user.Id, user.UserName ?? id.ToString(), "User");
            await transaction.CommitAsync();

            _logger.LogInformation($"Użytkownik {currentUser?.UserName} odblokował konto użytkownika {user.UserName}.");
            return Partial("_UserRows", new List<User> { user });
        }
    }
}
