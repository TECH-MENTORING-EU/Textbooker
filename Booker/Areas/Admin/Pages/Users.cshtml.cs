using Booker.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Booker.Areas.Admin.Pages
{
    public class UsersModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        public UsersModel(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public record LockoutLinkModel(int UserId, string? UserName, bool ShouldLockout);

        public List<User> Users { get; set; } = [];

        [FromQuery]
        public string? SearchTerm { get; set; } = string.Empty;
        public IActionResult OnGet()
        {
            Users = _userManager.Users.ToList();

            return Page();
        }

        public IActionResult OnGetSearch(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                Users = _userManager.Users.ToList();
            }
            else
            {
                Users = _userManager.Users
                    .Where(u => u.UserName!.Contains(searchTerm) || u.Email!.Contains(searchTerm))
                    .ToList();
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

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                // Handle deletion failure (e.g., log the error, display a message, etc.)
                ModelState.AddModelError(string.Empty, "Error deleting user.");
                Users = _userManager.Users.ToList();
                return new StatusCodeResult(500);
            }

            return Content("User deleted successfully.");
        }

        public async Task<IActionResult> OnPostLockoutAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

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

            var result = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
            if (!result.Succeeded)
            {
                // Handle lockout failure (e.g., log the error, display a message, etc.)
                ModelState.AddModelError(string.Empty, "Error locking out user.");
                Users = _userManager.Users.ToList();
                return new StatusCodeResult(500);
            }

            return Partial("_UserRows", new List<User>{user});
        }

        public async Task<IActionResult> OnPostUnlockAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
            if (!result.Succeeded)
            {
                // Handle unlock failure (e.g., log the error, display a message, etc.)
                ModelState.AddModelError(string.Empty, "Error unlocking user.");
                Users = _userManager.Users.ToList();
                return new StatusCodeResult(500);
            }

            return Partial("_UserRows", new List<User> { user });
        }
    }
}
