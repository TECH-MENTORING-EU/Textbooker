using Booker.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Booker.Services
{
    public interface IRatingManager
    {
        Task<(bool Success, string? Error)> AddRatingAsync(int reviewerId, int revieweeId, int value, string? comment);
        Task<double> GetAverageRatingAsync(int userId);
        Task<int> GetRatingCountAsync(int userId);
        Task<List<UserRating>> GetRatingsForUserAsync(int userId);
        Task<UserRating?> GetRatingAsync(int reviewerId, int revieweeId);
        Task<UserRating?> GetRatingByIdAsync(int ratingId);
        Task<bool> DeleteRatingAsync(int ratingId, int userId, bool isAdmin);
        Task<(bool Success, string? Error)> AddReplyAsync(int ratingId, int revieweeId, string reply);
        Task<bool> CanRateAsync(int reviewerId, int revieweeId);
    }

    public class RatingManager(DataContext context) : IRatingManager
    {
        public async Task<(bool Success, string? Error)> AddRatingAsync(int reviewerId, int revieweeId, int value, string? comment)
        {
            if (reviewerId == revieweeId)
                return (false, "Nie możesz ocenić samego siebie.");

            if (value < 1 || value > 5)
                return (false, "Ocena musi być w zakresie od 1 do 5.");

            var exists = await context.UserRatings
                .AnyAsync(ur => ur.ReviewerId == reviewerId && ur.RevieweeId == revieweeId);
            if (exists)
                return (false, "Już oceniłeś tego użytkownika.");

            var canRate = await CanRateAsync(reviewerId, revieweeId);
            if (!canRate)
                return (false, "Możesz ocenić sprzedającego tylko po zakończonej transakcji (zakup jego przedmiotu).");

            var rating = new UserRating
            {
                ReviewerId = reviewerId,
                RevieweeId = revieweeId,
                RatingValue = value,
                Comment = comment,
                CreatedAt = DateTime.UtcNow
            };

            context.UserRatings.Add(rating);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
            {
                // Unique index IX_UserRatings_ReviewerId_RevieweeId: a concurrent
                // rating for the same pair slipped past the pre-check above.
                return (false, "Już oceniłeś tego użytkownika.");
            }
            return (true, null);
        }

        public async Task<double> GetAverageRatingAsync(int userId)
        {
            return await context.UserRatings
                .Where(ur => ur.RevieweeId == userId)
                .AverageAsync(ur => (double?)ur.RatingValue) ?? 0;
        }

        public async Task<int> GetRatingCountAsync(int userId)
        {
            return await context.UserRatings.CountAsync(ur => ur.RevieweeId == userId);
        }

        public async Task<List<UserRating>> GetRatingsForUserAsync(int userId)
        {
            return await context.UserRatings
                .Include(ur => ur.Reviewer)
                .Where(ur => ur.RevieweeId == userId)
                .OrderByDescending(ur => ur.CreatedAt)
                .ToListAsync();
        }

        public async Task<UserRating?> GetRatingAsync(int reviewerId, int revieweeId)
        {
            return await context.UserRatings
                .FirstOrDefaultAsync(ur => ur.ReviewerId == reviewerId && ur.RevieweeId == revieweeId);
        }

        public async Task<UserRating?> GetRatingByIdAsync(int ratingId)
        {
            return await context.UserRatings.FindAsync(ratingId);
        }

        public async Task<bool> DeleteRatingAsync(int ratingId, int userId, bool isAdmin)
        {
            var rating = await context.UserRatings.FindAsync(ratingId);
            if (rating == null) return false;

            if (!isAdmin && rating.ReviewerId != userId) return false;

            context.UserRatings.Remove(rating);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool Success, string? Error)> AddReplyAsync(int ratingId, int revieweeId, string reply)
        {
            if (string.IsNullOrWhiteSpace(reply))
                return (false, "Odpowiedź nie może być pusta.");

            if (reply.Trim().Length > 500)
                return (false, "Odpowiedź nie może przekraczać 500 znaków.");

            var rating = await context.UserRatings.FindAsync(ratingId);
            if (rating == null)
                return (false, "Ocena nie została znaleziona.");

            if (rating.RevieweeId != revieweeId)
                return (false, "Tylko oceniany użytkownik może odpowiedzieć na ocenę.");

            if (rating.Reply != null)
                return (false, "Już odpowiedziałeś na tę ocenę.");

            rating.Reply = reply.Trim();
            rating.RepliedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<bool> CanRateAsync(int reviewerId, int revieweeId)
        {
            // Ratings are one-directional: only the BUYER rates the SELLER, and only
            // after a completed transaction. The seller names the buyer when
            // confirming the sale (SoldToUserId); a reservation alone, a chat
            // thread alone, or an auto-closed reservation never qualifies.
            return await context.Items
                .AnyAsync(i => i.UserId == revieweeId
                    && i.IsSold
                    && i.SoldToUserId == reviewerId);
        }
    }
}
