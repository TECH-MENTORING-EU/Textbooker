using System.ComponentModel.DataAnnotations;
using Booker.Data;
using Booker.Services;
using Booker.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Booker.Pages.Rating
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly IRatingManager _ratingManager;

        public CreateModel(UserManager<User> userManager, IRatingManager ratingManager)
        {
            _userManager = userManager;
            _ratingManager = ratingManager;
        }

        [BindProperty(SupportsGet = true)]
        public int UserId { get; set; }

        [BindProperty]
        [Range(1, 5)]
        public int RatingValue { get; set; }

        [BindProperty]
        [MaxLength(500)]
        public string? Comment { get; set; }

        public string RevieweeUserName { get; set; } = string.Empty;
        public bool CanRate { get; set; }
        public UserRating? ExistingRating { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var reviewee = await _userManager.FindByIdAsync(UserId.ToString());
            if (reviewee == null || !reviewee.IsVisible)
                return NotFound();

            var reviewerId = _userManager.GetUserId(User)!.IntOrDefault();
            if (reviewerId == -1)
                return Redirect("/Identity/Account/Login");

            if (reviewerId == UserId)
                return RedirectToPage("/Profile/Index", new { id = UserId });

            RevieweeUserName = reviewee.UserName!;
            ExistingRating = await _ratingManager.GetRatingAsync(reviewerId, UserId);
            CanRate = await _ratingManager.CanRateAsync(reviewerId, UserId);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var reviewerId = _userManager.GetUserId(User)!.IntOrDefault();
            if (reviewerId == -1)
                return Redirect("/Identity/Account/Login");

            var reviewee = await _userManager.FindByIdAsync(UserId.ToString());
            if (reviewee == null || !reviewee.IsVisible)
                return NotFound();

            RevieweeUserName = reviewee.UserName!;
            CanRate = await _ratingManager.CanRateAsync(reviewerId, UserId);

            if (!ModelState.IsValid)
                return Page();

            var result = await _ratingManager.AddRatingAsync(reviewerId, UserId, RatingValue, Comment?.Trim());

            if (!result.Success)
            {
                ErrorMessage = result.Error;
                return Page();
            }

            return RedirectToPage("/Profile/Index", new { id = UserId });
        }
    }
}
