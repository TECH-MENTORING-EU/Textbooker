using Booker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        public List<Subject> Subjects { get; set; } = new();
        public List<int> RecentItemIds { get; set; } = new();
        public List<HeroItem> HeroItems { get; set; } = new();

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

            // One query feeds both the hero and the gallery; hidden items are
            // excluded in SQL so the landing page always shows a full set.
            var landingItems = await _itemManager.GetRecentItemsAsync(12, currentUser);

            RecentItemIds = landingItems.Take(8).Select(i => i.Id).ToList();

            HeroItems = landingItems.Select(i => new HeroItem(
                i.Book.Title,
                i.Price.ToString("F2") + " zł",
                !string.IsNullOrEmpty(i.Photo)
                    ? _photosManager.GetPhotoUrl(i.Photo.Split(';')[0].Trim())
                    : ""
            )).ToList();

            return Page();
        }
    }
}
