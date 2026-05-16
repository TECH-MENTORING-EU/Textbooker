using Booker.Data;
using Booker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Booker.Areas.Admin.Pages
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ItemManager _itemManager;

        public IndexModel(UserManager<User> userManager, ItemManager itemManager)
        {
            _userManager = userManager;
            _itemManager = itemManager;
        }

        public int TotalUserCount { get; set; }
        public int ThisWeekNewUserCount { get; set; }
        public int TodayActiveUserCount { get; set; }

        public int TotalItemCount { get; set; }
        public int ThisWeekItemCount { get; set; }
        public int TodayItemCount { get; set; }


        public async Task<IActionResult> OnGetAsync()
        {
            TotalUserCount = await _userManager.Users.CountAsync();
            ThisWeekNewUserCount = await _userManager.Users
                .Where(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-7))
                .CountAsync();
            TodayActiveUserCount = await _userManager.Users
                .Where(u => u.LastActiveAt >= DateTime.UtcNow.AddDays(-1))
                .CountAsync();


            TotalItemCount = await _itemManager.GetAllItemsCountAsync();

            return Page();
        }
    }
}
