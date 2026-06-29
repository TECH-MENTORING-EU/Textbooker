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
        private readonly PhotosManager _photosManager;

        public List<Subject> Subjects { get; set; } = null!;
        public List<int> RecentItemIds { get; set; } = null!;
        public List<HeroItem> HeroItems { get; set; } = null!;

        public IndexModel(
            StaticDataManager staticDataManager,
            ItemManager itemManager,
            UserManager<User> userManager,
            PhotosManager photosManager
        )
        {
            _staticDataManager = staticDataManager;
            _itemManager = itemManager;
            _userManager = userManager;
            _photosManager = photosManager;
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

            RecentItemIds = await _itemManager
                .GetItemIdsByParamsAsync(params2, currentUser)
                .Take(4)
                .ToListAsync();

            var heroIds = await _itemManager
                .GetItemIdsByParamsAsync(params2, currentUser)
                .Take(12)
                .ToListAsync();

            HeroItems = await _itemManager
                .GetItemsByIdsAsync(heroIds, currentUser)
                .Where(i => i.IsVisible)
                .Take(12)
                .Select(i => new HeroItem(
                    i.Book.Title,
                    i.Price.ToString("F2") + " zł",
                    i.Photo != null && i.Photo.Length > 0
                        ? _photosManager.GetPhotoUrl(i.Photo.Split(';')[0].Trim())
                        : ""
                ))
                .ToListAsync();

            return Page();
        }
    }
}
