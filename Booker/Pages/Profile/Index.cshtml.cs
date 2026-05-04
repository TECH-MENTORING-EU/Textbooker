using Booker.Data;
using Booker.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Booker.Utilities;
using System.ComponentModel.DataAnnotations;

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

        public record UserModel(User RequestUser, bool IsCurrentUser);
        public UserModel UserInfo { get; set; } = null!;

        public double AverageRating { get; set; }
        public int RatingCount { get; set; }
        public List<UserRating> UserRatings { get; set; } = new();

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
            UserRatings = await _ratingManager.GetRatingsForUserAsync(Id.Value);

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

        public class RatingInputModel
        {
            [Range(1, 5)]
            public int RatingValue { get; set; }
            public string? Comment { get; set; }
        }

        [BindProperty]
        public RatingInputModel RatingInput { get; set; } = null!;

        public async Task<IActionResult> OnPostAddRatingAsync()
        {
            var currentUserId = _userManager.GetUserId(User).IntOrDefault();
            if (currentUserId == 0) return RedirectToPage("/Identity/Account/Login");
            if (!Id.HasValue) return NotFound();

            if (!ModelState.IsValid) return Page();

            var success = await _ratingManager.AddRatingAsync(currentUserId, Id.Value, RatingInput.RatingValue, RatingInput.Comment);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "You cannot rate yourself or rate the same user more than once.");
                return Page();
            }

            return RedirectToPage(new { id = Id });
        }
    }
}