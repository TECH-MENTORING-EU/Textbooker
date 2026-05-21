using Booker.Data;
using Booker.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Booker.Areas.Admin.Pages
{
    public class RatingsModel : PageModel
    {
        private readonly IRatingManager _ratingManager;
        private readonly DataContext _context;

        private const int PageSize = 25;

        public RatingsModel(IRatingManager ratingManager, DataContext context)
        {
            _ratingManager = ratingManager;
            _context = context;
        }

        public List<UserRating> Ratings { get; set; } = new();
        public int TotalRatingCount { get; set; }
        public double GlobalAverage { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            CurrentPage = pageNumber;

            TotalRatingCount = await _context.UserRatings.CountAsync();
            GlobalAverage = await _context.UserRatings.AnyAsync()
                ? Math.Round(await _context.UserRatings.AverageAsync(r => (double)r.RatingValue), 1)
                : 0;

            TotalPages = (int)Math.Ceiling(TotalRatingCount / (double)PageSize);

            Ratings = await _context.UserRatings
                .Include(r => r.Reviewer)
                .Include(r => r.Reviewee)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int ratingId)
        {
            await _ratingManager.DeleteRatingAsync(ratingId, 0, isAdmin: true);
            return RedirectToPage();
        }
    }
}
