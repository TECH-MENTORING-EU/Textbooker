using Booker.Data;
using Booker.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Booker.Tests;

public class RatingManagerTests
{
    private DataContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new DataContext(options);
        SeedUsers(context);
        return context;
    }

    private void SeedUsers(DataContext context)
    {
        var users = new List<User>
        {
            new() { Id = 1, UserName = "Alice", Email = "alice@test.com", IsVisible = true },
            new() { Id = 2, UserName = "Bob", Email = "bob@test.com", IsVisible = true },
            new() { Id = 3, UserName = "Charlie", Email = "charlie@test.com", IsVisible = true },
        };

        context.Users.AddRange(users);
        context.SaveChanges();
    }

    private void SeedSoldTransaction(DataContext context, int sellerId, int buyerId, bool sold = true)
    {
        var book = new Book
        {
            Id = 1,
            Title = "Test Book",
            Grades = new List<Grade>(),
            Subject = new Subject { Id = 1, Name = "Math" },
            Level = new Level { Id = 1, Name = "Basic" },
        };

        var seller = context.Users.Find(sellerId)!;

        context.Items.Add(new Item
        {
            Id = 1,
            Book = book,
            User = seller,
            UserId = sellerId,
            Price = 10m,
            CreatedAt = DateTime.UtcNow,
            Description = "Test",
            State = "Good",
            Photo = "",
            Reserved = false,
            IsSold = sold,
            SoldAt = sold ? DateTime.UtcNow : null,
            ReservedAt = DateTime.UtcNow.AddDays(-30),
        });

        context.ChatThreads.Add(new ChatThread
        {
            Id = 1,
            ChannelId = "ch-1",
            UserAId = buyerId,
            UserBId = sellerId,
            ItemId = 1,
        });

        context.SaveChanges();
    }

    // === AddRatingAsync ===

    [Fact]
    public async Task AddRatingAsync_Valid_ReturnsSuccess()
    {
        await using var context = CreateContext();
        SeedSoldTransaction(context, sellerId: 2, buyerId: 1);
        var manager = new RatingManager(context);

        var result = await manager.AddRatingAsync(1, 2, 4, "Great seller");

        Assert.True(result.Success);
        Assert.Null(result.Error);
        Assert.Single(context.UserRatings);
        Assert.Equal(4, context.UserRatings.First().RatingValue);
        Assert.Equal("Great seller", context.UserRatings.First().Comment);
    }

    [Fact]
    public async Task AddRatingAsync_SelfRating_ReturnsError()
    {
        await using var context = CreateContext();
        var manager = new RatingManager(context);

        var result = await manager.AddRatingAsync(1, 1, 5, null);

        Assert.False(result.Success);
        Assert.Contains("samego siebie", result.Error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    [InlineData(100)]
    public async Task AddRatingAsync_InvalidRange_ReturnsError(int value)
    {
        await using var context = CreateContext();
        SeedSoldTransaction(context, sellerId: 2, buyerId: 1);
        var manager = new RatingManager(context);

        var result = await manager.AddRatingAsync(1, 2, value, null);

        Assert.False(result.Success);
        Assert.Contains("1 do 5", result.Error);
    }

    [Fact]
    public async Task AddRatingAsync_Duplicate_ReturnsError()
    {
        await using var context = CreateContext();
        SeedSoldTransaction(context, sellerId: 2, buyerId: 1);
        var manager = new RatingManager(context);

        await manager.AddRatingAsync(1, 2, 4, null);
        var result = await manager.AddRatingAsync(1, 2, 3, null);

        Assert.False(result.Success);
        Assert.Contains("Już oceniłeś", result.Error);
    }

    [Fact]
    public async Task AddRatingAsync_NoReservedItem_ReturnsError()
    {
        await using var context = CreateContext();
        var manager = new RatingManager(context);

        var result = await manager.AddRatingAsync(1, 2, 4, null);

        Assert.False(result.Success);
        Assert.Contains("zakończonej transakcji", result.Error);
    }

    [Fact]
    public async Task AddRatingAsync_ReservedItemButNoChat_ReturnsError()
    {
        await using var context = CreateContext();
        var book = new Book
        {
            Id = 1, Title = "Test", Grades = new List<Grade>(),
            Subject = new Subject { Id = 1, Name = "Math" },
            Level = new Level { Id = 1, Name = "Basic" },
        };
        context.Items.Add(new Item
        {
            Id = 1, Book = book, User = context.Users.Find(2)!, UserId = 2,
            Price = 10m, CreatedAt = DateTime.UtcNow, Description = "", State = "", Photo = "", Reserved = true,
        });
        context.SaveChanges();

        var manager = new RatingManager(context);
        var result = await manager.AddRatingAsync(1, 2, 4, null);

        Assert.False(result.Success);
    }

    // === GetAverageRatingAsync ===

    [Fact]
    public async Task GetAverageRatingAsync_NoRatings_ReturnsZero()
    {
        await using var context = CreateContext();
        var manager = new RatingManager(context);

        var avg = await manager.GetAverageRatingAsync(1);

        Assert.Equal(0, avg);
    }

    [Fact]
    public async Task GetAverageRatingAsync_WithRatings_ReturnsCorrectAverage()
    {
        await using var context = CreateContext();
        context.UserRatings.AddRange(
            new UserRating { ReviewerId = 2, RevieweeId = 1, RatingValue = 3, CreatedAt = DateTime.UtcNow },
            new UserRating { ReviewerId = 3, RevieweeId = 1, RatingValue = 5, CreatedAt = DateTime.UtcNow }
        );
        context.SaveChanges();
        var manager = new RatingManager(context);

        var avg = await manager.GetAverageRatingAsync(1);

        Assert.Equal(4.0, avg);
    }

    // === GetRatingCountAsync ===

    [Fact]
    public async Task GetRatingCountAsync_NoRatings_ReturnsZero()
    {
        await using var context = CreateContext();
        var manager = new RatingManager(context);

        var count = await manager.GetRatingCountAsync(1);

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task GetRatingCountAsync_WithRatings_ReturnsCorrectCount()
    {
        await using var context = CreateContext();
        context.UserRatings.AddRange(
            new UserRating { ReviewerId = 2, RevieweeId = 1, RatingValue = 4, CreatedAt = DateTime.UtcNow },
            new UserRating { ReviewerId = 3, RevieweeId = 1, RatingValue = 5, CreatedAt = DateTime.UtcNow }
        );
        context.SaveChanges();
        var manager = new RatingManager(context);

        var count = await manager.GetRatingCountAsync(1);

        Assert.Equal(2, count);
    }

    // === GetMinMaxRatingAsync ===

    [Fact]
    public async Task GetMinMaxRatingAsync_NoRatings_ReturnsZeroZero()
    {
        await using var context = CreateContext();
        var manager = new RatingManager(context);

        var result = await manager.GetMinMaxRatingAsync(1);

        Assert.Equal(0, result.Min);
        Assert.Equal(0, result.Max);
    }

    [Fact]
    public async Task GetMinMaxRatingAsync_WithRatings_ReturnsCorrectRange()
    {
        await using var context = CreateContext();
        context.UserRatings.AddRange(
            new UserRating { ReviewerId = 2, RevieweeId = 1, RatingValue = 2, CreatedAt = DateTime.UtcNow },
            new UserRating { ReviewerId = 3, RevieweeId = 1, RatingValue = 5, CreatedAt = DateTime.UtcNow }
        );
        context.SaveChanges();
        var manager = new RatingManager(context);

        var result = await manager.GetMinMaxRatingAsync(1);

        Assert.Equal(2, result.Min);
        Assert.Equal(5, result.Max);
    }

    // === GetRatingsForUserAsync ===

    [Fact]
    public async Task GetRatingsForUserAsync_ReturnsOrderedByDateDescending()
    {
        await using var context = CreateContext();
        context.UserRatings.AddRange(
            new UserRating { ReviewerId = 2, RevieweeId = 1, RatingValue = 3, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new UserRating { ReviewerId = 3, RevieweeId = 1, RatingValue = 5, CreatedAt = DateTime.UtcNow }
        );
        context.SaveChanges();
        var manager = new RatingManager(context);

        var ratings = await manager.GetRatingsForUserAsync(1);

        Assert.Equal(2, ratings.Count);
        Assert.Equal(5, ratings[0].RatingValue);
        Assert.Equal(3, ratings[1].RatingValue);
    }

    // === GetRatingAsync ===

    [Fact]
    public async Task GetRatingAsync_Existing_ReturnsRating()
    {
        await using var context = CreateContext();
        context.UserRatings.Add(
            new UserRating { ReviewerId = 2, RevieweeId = 1, RatingValue = 4, CreatedAt = DateTime.UtcNow }
        );
        context.SaveChanges();
        var manager = new RatingManager(context);

        var rating = await manager.GetRatingAsync(2, 1);

        Assert.NotNull(rating);
        Assert.Equal(4, rating.RatingValue);
    }

    [Fact]
    public async Task GetRatingAsync_NotExisting_ReturnsNull()
    {
        await using var context = CreateContext();
        var manager = new RatingManager(context);

        var rating = await manager.GetRatingAsync(1, 2);

        Assert.Null(rating);
    }

    // === UpdateRatingAsync ===

    [Fact]
    public async Task UpdateRatingAsync_OwnerCanUpdate_ReturnsSuccess()
    {
        await using var context = CreateContext();
        context.UserRatings.Add(
            new UserRating { Id = 10, ReviewerId = 1, RevieweeId = 2, RatingValue = 3, CreatedAt = DateTime.UtcNow }
        );
        context.SaveChanges();
        var manager = new RatingManager(context);

        var result = await manager.UpdateRatingAsync(10, 1, 5, "Updated");

        Assert.True(result.Success);
        Assert.Null(result.Error);
        Assert.Equal(5, context.UserRatings.Find(10)!.RatingValue);
        Assert.Equal("Updated", context.UserRatings.Find(10)!.Comment);
    }

    [Fact]
    public async Task UpdateRatingAsync_NotOwner_ReturnsError()
    {
        await using var context = CreateContext();
        context.UserRatings.Add(
            new UserRating { Id = 10, ReviewerId = 1, RevieweeId = 2, RatingValue = 3, CreatedAt = DateTime.UtcNow }
        );
        context.SaveChanges();
        var manager = new RatingManager(context);

        var result = await manager.UpdateRatingAsync(10, 2, 5, null);

        Assert.False(result.Success);
        Assert.Contains("swoje oceny", result.Error);
    }

    [Fact]
    public async Task UpdateRatingAsync_InvalidRange_ReturnsError()
    {
        await using var context = CreateContext();
        context.UserRatings.Add(
            new UserRating { Id = 10, ReviewerId = 1, RevieweeId = 2, RatingValue = 3, CreatedAt = DateTime.UtcNow }
        );
        context.SaveChanges();
        var manager = new RatingManager(context);

        var result = await manager.UpdateRatingAsync(10, 1, 0, null);

        Assert.False(result.Success);
        Assert.Contains("1 do 5", result.Error);
    }

    [Fact]
    public async Task UpdateRatingAsync_NotFound_ReturnsError()
    {
        await using var context = CreateContext();
        var manager = new RatingManager(context);

        var result = await manager.UpdateRatingAsync(999, 1, 3, null);

        Assert.False(result.Success);
        Assert.Contains("nie została znaleziona", result.Error);
    }

    // === DeleteRatingAsync ===

    [Fact]
    public async Task DeleteRatingAsync_AuthorCanDelete_ReturnsTrue()
    {
        await using var context = CreateContext();
        context.UserRatings.Add(
            new UserRating { Id = 10, ReviewerId = 1, RevieweeId = 2, RatingValue = 3, CreatedAt = DateTime.UtcNow }
        );
        context.SaveChanges();
        var manager = new RatingManager(context);

        var deleted = await manager.DeleteRatingAsync(10, 1, isAdmin: false);

        Assert.True(deleted);
        Assert.Empty(context.UserRatings);
    }

    [Fact]
    public async Task DeleteRatingAsync_AdminCanDelete_ReturnsTrue()
    {
        await using var context = CreateContext();
        context.UserRatings.Add(
            new UserRating { Id = 10, ReviewerId = 1, RevieweeId = 2, RatingValue = 3, CreatedAt = DateTime.UtcNow }
        );
        context.SaveChanges();
        var manager = new RatingManager(context);

        var deleted = await manager.DeleteRatingAsync(10, 999, isAdmin: true);

        Assert.True(deleted);
        Assert.Empty(context.UserRatings);
    }

    [Fact]
    public async Task DeleteRatingAsync_NotAuthorNotAdmin_ReturnsFalse()
    {
        await using var context = CreateContext();
        context.UserRatings.Add(
            new UserRating { Id = 10, ReviewerId = 1, RevieweeId = 2, RatingValue = 3, CreatedAt = DateTime.UtcNow }
        );
        context.SaveChanges();
        var manager = new RatingManager(context);

        var deleted = await manager.DeleteRatingAsync(10, 2, isAdmin: false);

        Assert.False(deleted);
        Assert.Single(context.UserRatings);
    }

    [Fact]
    public async Task DeleteRatingAsync_NotFound_ReturnsFalse()
    {
        await using var context = CreateContext();
        var manager = new RatingManager(context);

        var deleted = await manager.DeleteRatingAsync(999, 1, isAdmin: false);

        Assert.False(deleted);
    }

    // === AddReplyAsync ===

    [Fact]
    public async Task AddReplyAsync_Valid_ReturnsSuccess()
    {
        await using var context = CreateContext();
        context.UserRatings.Add(
            new UserRating { Id = 10, ReviewerId = 2, RevieweeId = 1, RatingValue = 4, CreatedAt = DateTime.UtcNow }
        );
        context.SaveChanges();
        var manager = new RatingManager(context);

        var result = await manager.AddReplyAsync(10, 1, "Thanks!");

        Assert.True(result.Success);
        Assert.Null(result.Error);
        var rating = context.UserRatings.Find(10)!;
        Assert.Equal("Thanks!", rating.Reply);
        Assert.NotNull(rating.RepliedAt);
    }

    [Fact]
    public async Task AddReplyAsync_EmptyReply_ReturnsError()
    {
        await using var context = CreateContext();
        context.UserRatings.Add(
            new UserRating { Id = 10, ReviewerId = 2, RevieweeId = 1, RatingValue = 4, CreatedAt = DateTime.UtcNow }
        );
        context.SaveChanges();
        var manager = new RatingManager(context);

        var result = await manager.AddReplyAsync(10, 1, "   ");

        Assert.False(result.Success);
        Assert.Contains("pusta", result.Error);
    }

    [Fact]
    public async Task AddReplyAsync_WrongUser_ReturnsError()
    {
        await using var context = CreateContext();
        context.UserRatings.Add(
            new UserRating { Id = 10, ReviewerId = 2, RevieweeId = 1, RatingValue = 4, CreatedAt = DateTime.UtcNow }
        );
        context.SaveChanges();
        var manager = new RatingManager(context);

        var result = await manager.AddReplyAsync(10, 2, "Hack");

        Assert.False(result.Success);
        Assert.Contains("oceniany użytkownik", result.Error);
    }

    [Fact]
    public async Task AddReplyAsync_AlreadyReplied_ReturnsError()
    {
        await using var context = CreateContext();
        context.UserRatings.Add(
            new UserRating
            {
                Id = 10, ReviewerId = 2, RevieweeId = 1, RatingValue = 4, CreatedAt = DateTime.UtcNow,
                Reply = "First", RepliedAt = DateTime.UtcNow
            }
        );
        context.SaveChanges();
        var manager = new RatingManager(context);

        var result = await manager.AddReplyAsync(10, 1, "Second");

        Assert.False(result.Success);
        Assert.Contains("Już odpowiedziałeś", result.Error);
    }

    [Fact]
    public async Task AddReplyAsync_RatingNotFound_ReturnsError()
    {
        await using var context = CreateContext();
        var manager = new RatingManager(context);

        var result = await manager.AddReplyAsync(999, 1, "Test");

        Assert.False(result.Success);
        Assert.Contains("nie została znaleziona", result.Error);
    }

    [Fact]
    public async Task AddReplyAsync_TrimsWhitespace()
    {
        await using var context = CreateContext();
        context.UserRatings.Add(
            new UserRating { Id = 10, ReviewerId = 2, RevieweeId = 1, RatingValue = 4, CreatedAt = DateTime.UtcNow }
        );
        context.SaveChanges();
        var manager = new RatingManager(context);

        await manager.AddReplyAsync(10, 1, "  Hello  ");

        Assert.Equal("Hello", context.UserRatings.Find(10)!.Reply);
    }

    // === CanRateAsync ===
    // New semantics: a rating requires a completed transaction — the seller's item
    // linked to the shared chat thread is sold. Reserved-state alone never qualifies.

    [Fact]
    public async Task CanRateAsync_WithSoldItemAndThread_ReturnsTrue()
    {
        await using var context = CreateContext();
        SeedSoldTransaction(context, sellerId: 2, buyerId: 1);
        var manager = new RatingManager(context);

        var canRate = await manager.CanRateAsync(1, 2);

        Assert.True(canRate);
    }

    [Fact]
    public async Task CanRateAsync_NoItemsNoChat_ReturnsFalse()
    {
        await using var context = CreateContext();
        var manager = new RatingManager(context);

        var canRate = await manager.CanRateAsync(1, 2);

        Assert.False(canRate);
    }

    [Fact]
    public async Task CanRateAsync_ThreadButItemNotSold_ReturnsFalse()
    {
        await using var context = CreateContext();
        SeedSoldTransaction(context, sellerId: 2, buyerId: 1, sold: false);
        var manager = new RatingManager(context);

        var canRate = await manager.CanRateAsync(1, 2);

        Assert.False(canRate);
    }

    [Fact]
    public async Task CanRateAsync_SoldItemButNoThread_ReturnsFalse()
    {
        await using var context = CreateContext();
        var book = new Book
        {
            Id = 1, Title = "Test", Grades = new List<Grade>(),
            Subject = new Subject { Id = 1, Name = "Math" },
            Level = new Level { Id = 1, Name = "Basic" },
        };
        context.Items.Add(new Item
        {
            Id = 1, Book = book, User = context.Users.Find(2)!, UserId = 2,
            Price = 10m, CreatedAt = DateTime.UtcNow, Description = "", State = "", Photo = "",
            IsSold = true, SoldAt = DateTime.UtcNow,
        });
        context.SaveChanges();
        var manager = new RatingManager(context);

        var canRate = await manager.CanRateAsync(1, 2);

        Assert.False(canRate);
    }

    [Fact]
    public async Task CanRateAsync_SoldItemButThreadWithOtherItem_ReturnsFalse()
    {
        await using var context = CreateContext();
        // The thread exists and user 2 has a sold item — but the thread points at
        // a different listing, so this sale does not justify the rating.
        var book = new Book
        {
            Id = 1, Title = "Test", Grades = new List<Grade>(),
            Subject = new Subject { Id = 1, Name = "Math" },
            Level = new Level { Id = 1, Name = "Basic" },
        };
        context.Items.Add(new Item
        {
            Id = 1, Book = book, User = context.Users.Find(2)!, UserId = 2,
            Price = 10m, CreatedAt = DateTime.UtcNow, Description = "", State = "", Photo = "",
            IsSold = true, SoldAt = DateTime.UtcNow,
        });
        context.Items.Add(new Item
        {
            Id = 2, Book = book, User = context.Users.Find(2)!, UserId = 2,
            Price = 5m, CreatedAt = DateTime.UtcNow, Description = "", State = "", Photo = "",
            IsSold = false,
        });
        context.ChatThreads.Add(new ChatThread { Id = 1, ChannelId = "ch-1", UserAId = 1, UserBId = 2, ItemId = 2 });
        context.SaveChanges();
        var manager = new RatingManager(context);

        var canRate = await manager.CanRateAsync(1, 2);

        Assert.False(canRate);
    }

    [Fact]
    public async Task CanRateAsync_SellerCannotRateBuyer_EvenAfterSoldItem()
    {
        // Ratings are one-directional: buyer rates seller. The seller of the sold
        // item cannot rate back.
        await using var context = CreateContext();
        SeedSoldTransaction(context, sellerId: 2, buyerId: 1);
        var manager = new RatingManager(context);

        var canRate = await manager.CanRateAsync(2, 1);

        Assert.False(canRate);
    }

    [Fact]
    public async Task AddRatingAsync_ReservedButNotSoldItemInThread_ReturnsError()
    {
        // Pins the new semantics on AddRatingAsync itself (not just CanRateAsync):
        // a shared thread about an item that is only reserved — not sold — never
        // qualifies, under old or new rules alike.
        await using var context = CreateContext();
        SeedSoldTransaction(context, sellerId: 2, buyerId: 1, sold: false);
        var manager = new RatingManager(context);

        var result = await manager.AddRatingAsync(1, 2, 5, "great");

        Assert.False(result.Success);
        Assert.Contains("zakończonej transakcji", result.Error);
    }

    [Fact]
    public async Task CanRateAsync_ThreadWithoutItem_ReturnsFalse()
    {
        // Legacy bare threads (no listing attached) never enable ratings, even
        // between two users who already completed some other transaction.
        await using var context = CreateContext();
        SeedSoldTransaction(context, sellerId: 2, buyerId: 1);
        context.ChatThreads.Add(new ChatThread { Id = 2, ChannelId = "ch-2", UserAId = 1, UserBId = 2, ItemId = null });
        context.SaveChanges();
        var manager = new RatingManager(context);

        var canRate = await manager.CanRateAsync(1, 2);

        Assert.True(canRate); // via the sold item in thread ch-1
        context.ChatThreads.Remove(context.ChatThreads.Find(1)!);
        context.SaveChanges();

        Assert.False(await manager.CanRateAsync(1, 2)); // only the bare thread left
    }

    [Fact]
    public async Task AddRatingAsync_AfterAutoClose_EnablesRating()
    {
        // End-to-end: reservation auto-closed after 30 days (no seller decision)
        // still counts as a completed transaction for the rating gate.
        await using var context = CreateContext(); // users 1 (buyer), 2 (seller) seeded
        var book = new Book { Id = 1, Title = "T", Grades = new List<Grade>(), Subject = new Subject { Id = 1, Name = "M" }, Level = new Level { Id = 1, Name = "B" } };
        context.Items.Add(new Item
        {
            Id = 1, Book = book, User = context.Users.Find(2)!, UserId = 2,
            Price = 45m, CreatedAt = DateTime.UtcNow.AddDays(-31), Description = "", State = "Good", Photo = "",
            Reserved = true, ReservedAt = DateTime.UtcNow.AddDays(-31),
        });
        context.ChatThreads.Add(new ChatThread { Id = 1, ChannelId = "ch-1", UserAId = 1, UserBId = 2, ItemId = 1 });
        context.SaveChanges();

        var itemManager = new ItemManager(context, null!, null!, null!);
        await itemManager.AutoCloseStaleReservationsAsync();

        var manager = new RatingManager(context);
        var result = await manager.AddRatingAsync(1, 2, 4, null);

        Assert.True(result.Success);
    }
}
