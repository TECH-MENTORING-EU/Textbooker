using Booker.Data;
using Microsoft.EntityFrameworkCore;

namespace Booker.Services
{
    public interface IRatingManager
    {
        Task<(bool Success, string? Error)> AddRatingAsync(int reviewerId, int revieweeId, int value, string? comment);
        Task<double> GetAverageRatingAsync(int userId);
        Task<int> GetRatingCountAsync(int userId);
        Task<(int Min, int Max)> GetMinMaxRatingAsync(int userId);
        Task<List<UserRating>> GetRatingsForUserAsync(int userId);
        Task<UserRating?> GetRatingAsync(int reviewerId, int revieweeId);
        Task<UserRating?> GetRatingByIdAsync(int ratingId);
        Task<(bool Success, string? Error)> UpdateRatingAsync(int ratingId, int userId, int value, string? comment);
        Task<bool> DeleteRatingAsync(int ratingId, int userId, bool isAdmin);
        Task<(bool Success, string? Error)> AddReplyAsync(int ratingId, int revieweeId, string reply);
        Task<bool> CanRateAsync(int reviewerId, int revieweeId);
    }

    public class RatingManager : IRatingManager
    {
        private readonly DataContext _context;

        public RatingManager(DataContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string? Error)> AddRatingAsync(int reviewerId, int revieweeId, int value, string? comment)
        {
            if (reviewerId == revieweeId)
                return (false, "Nie możesz ocenić samego siebie.");

            if (value < 1 || value > 5)
                return (false, "Ocena musi być w zakresie od 1 do 5.");

            var exists = await _context.UserRatings
                .AnyAsync(ur => ur.ReviewerId == reviewerId && ur.RevieweeId == revieweeId);
            if (exists)
                return (false, "Już oceniłeś tego użytkownika.");

            var canRate = await CanRateAsync(reviewerId, revieweeId);
            if (!canRate)
                return (false, "Możesz ocenić tylko użytkownika, z którym miałeś interakcję dotyczącą zarezerwowanej książki.");

            var rating = new UserRating
            {
                ReviewerId = reviewerId,
                RevieweeId = revieweeId,
                RatingValue = value,
                Comment = comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserRatings.Add(rating);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<double> GetAverageRatingAsync(int userId)
        {
            return await _context.UserRatings
                .Where(ur => ur.RevieweeId == userId)
                .AverageAsync(ur => (double?)ur.RatingValue) ?? 0;
        }

        public async Task<int> GetRatingCountAsync(int userId)
        {
            return await _context.UserRatings.CountAsync(ur => ur.RevieweeId == userId);
        }

        public async Task<(int Min, int Max)> GetMinMaxRatingAsync(int userId)
        {
            var ratings = await _context.UserRatings
                .Where(ur => ur.RevieweeId == userId)
                .Select(ur => ur.RatingValue)
                .ToListAsync();

            if (ratings.Count == 0)
                return (0, 0);

            return (ratings.Min(), ratings.Max());
        }

        public async Task<List<UserRating>> GetRatingsForUserAsync(int userId)
        {
            return await _context.UserRatings
                .Include(ur => ur.Reviewer)
                .Where(ur => ur.RevieweeId == userId)
                .OrderByDescending(ur => ur.CreatedAt)
                .ToListAsync();
        }

        public async Task<UserRating?> GetRatingAsync(int reviewerId, int revieweeId)
        {
            return await _context.UserRatings
                .FirstOrDefaultAsync(ur => ur.ReviewerId == reviewerId && ur.RevieweeId == revieweeId);
        }

        public async Task<UserRating?> GetRatingByIdAsync(int ratingId)
        {
            return await _context.UserRatings.FindAsync(ratingId);
        }

        public async Task<(bool Success, string? Error)> UpdateRatingAsync(int ratingId, int userId, int value, string? comment)
        {
            if (value < 1 || value > 5)
                return (false, "Ocena musi być w zakresie od 1 do 5.");

            var rating = await _context.UserRatings.FindAsync(ratingId);
            if (rating == null)
                return (false, "Ocena nie została znaleziona.");

            if (rating.ReviewerId != userId)
                return (false, "Możesz edytować tylko swoje oceny.");

            rating.RatingValue = value;
            rating.Comment = comment;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<bool> DeleteRatingAsync(int ratingId, int userId, bool isAdmin)
        {
            var rating = await _context.UserRatings.FindAsync(ratingId);
            if (rating == null) return false;

            if (!isAdmin && rating.ReviewerId != userId) return false;

            _context.UserRatings.Remove(rating);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool Success, string? Error)> AddReplyAsync(int ratingId, int revieweeId, string reply)
        {
            if (string.IsNullOrWhiteSpace(reply))
                return (false, "Odpowiedź nie może być pusta.");

            var rating = await _context.UserRatings.FindAsync(ratingId);
            if (rating == null)
                return (false, "Ocena nie została znaleziona.");

            if (rating.RevieweeId != revieweeId)
                return (false, "Tylko oceniany użytkownik może odpowiedzieć na ocenę.");

            if (rating.Reply != null)
                return (false, "Już odpowiedziałeś na tę ocenę.");

            rating.Reply = reply.Trim();
            rating.RepliedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<bool> CanRateAsync(int reviewerId, int revieweeId)
        {
            var hasChatThread = await _context.ChatThreads
                .AnyAsync(t =>
                    (t.UserAId == reviewerId && t.UserBId == revieweeId) ||
                    (t.UserAId == revieweeId && t.UserBId == reviewerId));

            if (!hasChatThread)
                return false;

            // Check if either user has a reserved item — they must have transacted
            var hasReservedItem = await _context.Items
                .AnyAsync(i => i.UserId == revieweeId && i.Reserved);

            var reviewerHasReservedItem = await _context.Items
                .AnyAsync(i => i.UserId == reviewerId && i.Reserved);

            return hasReservedItem || reviewerHasReservedItem;
        }
    }
}
