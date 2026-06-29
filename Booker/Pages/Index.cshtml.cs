using Booker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Booker.Services;
using Microsoft.AspNetCore.Identity;

namespace Booker.Pages
{
    public class IndexModel : PageModel
    {
        private readonly StaticDataManager _staticDataManager;
        private readonly ItemManager _itemManager;
        private readonly UserManager<User> _userManager;

        public List<Subject> Subjects { get; set; } = null!;
        public List<int> RecentItemIds { get; set; } = null!;

        public IndexModel(
            StaticDataManager staticDataManager,
            ItemManager itemManager,
            UserManager<User> userManager
        )
        {
            _staticDataManager = staticDataManager;
            _itemManager = itemManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            Subjects = await _staticDataManager.GetSubjectsAsync();

            var currentUser = User.Identity?.IsAuthenticated == true
                ? await _userManager.GetUserAsync(User)
                : null;

            var params2 = new ItemManager.Parameters(
                Search: null,
                Grades: new(),
                Subject: null,
                Level: null,
                MinPrice: null,
                MaxPrice: null
            );

            RecentItemIds = await _itemManager
                .GetItemIdsByParamsAsync(params2, currentUser)
                .Take(8)
                .ToListAsync();

            return Page();
        }
    }
}
