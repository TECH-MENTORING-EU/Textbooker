using Booker.Data;
using Microsoft.EntityFrameworkCore;

namespace Booker.Services
{
    public interface IRatingManager
    {
        Task<bool> AddRatingAsync(int reviewerId, int revieweeId, int value, string? comment);
        Task<double> GetAverageRatingAsync(int userId);
        Task<int> GetRatingCountAsync(int userId);
        Task<List<UserRating>> GetRatingsForUserAsync(int userId);
    }

    public class RatingManager : IRatingManager
    {
        private readonly DataContext _context;

        public RatingManager(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> AddRatingAsync(int reviewerId, int revieweeId, int value, string? comment)
        {
            if (reviewerId == revieweeId) return false;

            var exists = await _context.UserRatings.AnyAsync(ur => ur.ReviewerId == reviewerId && ur.RevieweeId == revieweeId);
            if (exists) return false;

            var rating = new UserRating
            {
                ReviewerId = reviewerId,
                RevieweeId = revieweeId,
                RatingValue = value,
                Comment = comment,
                CreatedAt = DateTime.Now
            };

            _context.UserRatings.Add(rating);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<double> GetAverageRatingAsync(int userId)
        {
            var ratings = await _context.UserRatings
                .Where(ur => ur.RevieweeId == userId)
                .Select(ur => ur.RatingValue)
                .ToListAsync();

            return ratings.Any() ? ratings.Average() : 0;
        }

        public async Task<int> GetRatingCountAsync(int userId)
        {
            return await _context.UserRatings.CountAsync(ur => ur.RevieweeId == userId);
        }

        public async Task<List<UserRating>> GetRatingsForUserAsync(int userId)
        {
            return await _context.UserRatings
                .Include(ur => ur.Reviewer)
                .Where(ur => ur.RevieweeId == userId)
                .OrderByDescending(ur => ur.CreatedAt)
                .ToListAsync();
        }
    }
}
