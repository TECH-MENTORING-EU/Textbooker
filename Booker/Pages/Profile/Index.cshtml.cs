using Booker.Data;
using Booker.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Booker.Utilities;

namespace Booker.Pages.Profile
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly UserManager<User> _userManager;
        private readonly ItemManager _itemManager;
        private readonly IRatingManager _ratingManager;
        const int PageSize = 25;

        public IndexModel(ILogger<IndexModel> logger, UserManager<User> userManager, ItemManager itemManager, IRatingManager ratingManager)
        {
            _logger = logger;
            _userManager = userManager;
            _itemManager = itemManager;
            _ratingManager = ratingManager;
        }
        [FromRoute]
        public int? Id { get; set; }

        public List<int>? ItemIds { get; set; }
        public StaticDataManager.Parameters Params { get; set; } = null!;

        public double AverageRating { get; set; }
        public int RatingCount { get; set; }
        public List<UserRating> Ratings { get; set; } = new();
        public bool CanRate { get; set; }
        public bool HasExistingRating { get; set; }

        public record UserModel(User RequestUser, bool IsCurrentUser);
        public UserModel UserInfo { get; set; } = null!;
        public async Task<IActionResult> OnGetAsync(int pageNumber)
        {
            var currentUserId = _userManager.GetUserId(User).IntOrDefault();

            if (!Id.HasValue)
            {
                if (currentUserId == 0)
                {
                    return Redirect("/Identity/Account/Login");
                }

                Id = currentUserId;
            }            

            var user = await _userManager.FindByIdAsync(Id.Value.ToString());

            if (user == null || !user.IsVisible)
            {
                return NotFound();
            }

            ItemIds = await _itemManager.GetUserItemIdsAsync(Id.Value).ToListAsync();

            Params = new StaticDataManager.Parameters(null, [], null, null);

            UserInfo = new UserModel(user, user.Id == currentUserId);

            AverageRating = await _ratingManager.GetAverageRatingAsync(Id.Value);
            RatingCount = await _ratingManager.GetRatingCountAsync(Id.Value);
            Ratings = await _ratingManager.GetRatingsForUserAsync(Id.Value);

            if (!UserInfo.IsCurrentUser && currentUserId > 0)
            {
                CanRate = await _ratingManager.CanRateAsync(currentUserId, Id.Value);
                HasExistingRating = await _ratingManager.GetRatingAsync(currentUserId, Id.Value) != null;
            }

            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return ViewComponent("ItemGalleryViewComponent", new
                {
                    itemIds = ItemIds,
                    parameters = Params,
                    pageNumber = pageNumber,
                    showHidden = UserInfo.IsCurrentUser,
                });
            }
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteRatingAsync(int ratingId)
        {
            var userId = _userManager.GetUserId(User).IntOrDefault();
            if (userId == -1) return Forbid();

            var isAdmin = User.IsInRole("Admin");
            await _ratingManager.DeleteRatingAsync(ratingId, userId, isAdmin);
            return RedirectToPage();
        }
    }
}