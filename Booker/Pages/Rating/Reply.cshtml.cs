using Booker.Services;
using Booker.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Booker.Data;

namespace Booker.Pages.Rating
{
    [Authorize]
    public class ReplyModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly IRatingManager _ratingManager;

        public ReplyModel(UserManager<User> userManager, IRatingManager ratingManager)
        {
            _userManager = userManager;
            _ratingManager = ratingManager;
        }

        [BindProperty(SupportsGet = true)]
        public int RatingId { get; set; }

        [BindProperty]
        public string Reply { get; set; } = string.Empty;

        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = _userManager.GetUserId(User)!.IntOrDefault();
            if (userId == -1)
                return Redirect("/Identity/Account/Login");

            var result = await _ratingManager.AddReplyAsync(RatingId, userId, Reply);

            if (!result.Success)
            {
                TempData["ReplyError"] = result.Error;
            }

            var rating = await _ratingManager.GetRatingByIdAsync(RatingId);
            if (rating != null)
                return RedirectToPage("/Profile/Index", new { id = rating.RevieweeId });

            return RedirectToPage("/Index");
        }
    }
}
