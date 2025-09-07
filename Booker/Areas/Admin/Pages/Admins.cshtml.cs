using System.Threading.Tasks;
using Booker.Data;
using Booker.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Booker.Areas.Admin.Pages
{
    public class AdminsModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SessionCacheManager _sessionCacheManager;
        private readonly ILogger<AdminsModel> _logger;

        public AdminsModel(UserManager<User> userManager, SessionCacheManager sessionCacheManager, ILogger<AdminsModel> logger)
        {
            _userManager = userManager;
            _sessionCacheManager = sessionCacheManager;
            _logger = logger;
        }
        public List<User> Admins { get; set; } = [];
        public async Task<IActionResult> OnGetAsync()
        {
            Admins = (await _userManager.GetUsersInRoleAsync("Admin")).ToList();
            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "Nazwa użytkownika i hasło są wymagane.");
                return await OnGetAsync();
            }

            var user = await _userManager.FindByNameAsync(username);
            var currentUser = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Nie znaleziono użytkownika o podanej nazwie.");
                return await OnGetAsync();
            }

            var passwordCheck = await _userManager.CheckPasswordAsync(currentUser!, password);
            if (!passwordCheck)
            {
                ModelState.AddModelError(string.Empty, "Niepoprawne hasło.");
                _logger.LogWarning(
                    $"Użytkownik {currentUser?.UserName} próbował nadać uprawnienia administratora użytkownikowi {user.UserName}, ale wpisał błędne hasło."
                );
                return await OnGetAsync();
            }

            var isInRole = await _userManager.IsInRoleAsync(user, "Admin");
            if (isInRole)
            {
                ModelState.AddModelError(string.Empty, "Użytkownik jest już administratorem.");
                return await OnGetAsync();
            }

            var result = await _userManager.AddToRoleAsync(user, "Admin");
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Nie udało się dodać użytkownika do roli administratora.");
                return await OnGetAsync();
            }

            _logger.LogInformation($"Użytkownik {currentUser?.UserName} nadał uprawnienia administratora użytkownikowi {user.UserName}.");
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            var currentUser = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            if (user.Id == currentUser!.Id)
            {
                ModelState.AddModelError(string.Empty, "Nie możesz usunąć swojego własnego konta administratora.");
                return new NoContentResult();
            }

            var isInRole = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isInRole)
            {
                ModelState.AddModelError(string.Empty, "Użytkownik nie jest administratorem.");
                return new NoContentResult();
            }

            var result = await _userManager.RemoveFromRoleAsync(user, "Admin");
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Nie udało się usunąć użytkownika z roli administratora.");
                return new NoContentResult();
            }


            _sessionCacheManager.InvalidateSession(id);
            _logger.LogInformation($"Użytkownik {currentUser?.UserName} usunął uprawnienia administratora użytkownika {user.UserName}.");
            return Content("Administrator usunięty pomyślnie.");
        }
    }
}
