using Booker.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;


namespace Booker.Pages
{
    public class HowManyModel : PageModel
    {
        private readonly UserManager<User> _userManager;

        public int UserCount { get; set; } = 0;

        public HowManyModel(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(int? code)
        {
            if (code != 2137)
            {
                return NotFound();
            }

            UserCount = await _userManager.Users.CountAsync();

            return Page();
        }
    }
}
