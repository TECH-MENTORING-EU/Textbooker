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

        public List<Subject> Subjects { get; set; } = new();
        public List<int> RecentItemIds { get; set; } = new();
        public List<HeroItem> HeroItems { get; set; } = new();

        public IndexModel(
            ILogger<IndexModel> logger,
            ItemManager itemManager,
            StaticDataManager staticDataManager
            )
        {
            _staticDataManager = staticDataManager;
        }

        public record HeroItem(string Title, string Price, string Photo);

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

            ItemIds = await _itemManager.GetItemIdsByParamsAsync(params2).ToListAsync();

            return Page();
        }
    }
}
