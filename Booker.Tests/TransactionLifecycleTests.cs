using Booker.Data;
using Booker.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Booker.Tests;

/// <summary>
/// Tests for the transaction lifecycle: reserve → sale confirmation prompt (7d)
/// → auto-close (30d), and how the lifecycle gates ratings.
/// </summary>
public class TransactionLifecycleTests
{
    private DataContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new DataContext(options);
        context.Users.AddRange(
            new User { Id = 1, UserName = "Seller", Email = "seller@test.com", IsVisible = true },
            new User { Id = 2, UserName = "Buyer", Email = "buyer@test.com", IsVisible = true }
        );
        context.SaveChanges();
        return context;
    }

    private static Item NewItem(DataContext context, int sellerId, Action<Item>? customize = null)
    {
        var book = context.Books.Local.FirstOrDefault(b => b.Id == 1)
            ?? context.Books.FirstOrDefault(b => b.Id == 1)
            ?? new Book
            {
                Id = 1, Title = "Test Book", Grades = new List<Grade>(),
                Subject = context.Subjects.Local.FirstOrDefault(s => s.Id == 1)
                    ?? new Subject { Id = 1, Name = "Math" },
                Level = context.Levels.Local.FirstOrDefault(l => l.Id == 1)
                    ?? new Level { Id = 1, Name = "Basic" },
            };
        var item = new Item
        {
            Id = context.Items.Count() + 1,
            Book = book,
            User = context.Users.Find(sellerId)!,
            UserId = sellerId,
            Price = 10m,
            CreatedAt = DateTime.UtcNow,
            Description = "",
            State = "",
            Photo = "",
        };
        customize?.Invoke(item);
        context.Items.Add(item);
        context.SaveChanges();
        return item;
    }

    [Fact]
    public async Task MarkItemReservedAsync_SetsReservedAt()
    {
        await using var context = CreateContext();
        NewItem(context, sellerId: 1);
        var manager = new ItemManager(context, null!, null!, null!);

        await manager.MarkItemReservedAsync(1, true);

        var item = await context.Items.FindAsync(1);
        Assert.True(item!.Reserved);
        Assert.NotNull(item.ReservedAt);
    }

    [Fact]
    public async Task MarkItemReservedAsync_UnreserveClearsReservedAt()
    {
        await using var context = CreateContext();
        NewItem(context, sellerId: 1, i => { i.Reserved = true; i.ReservedAt = DateTime.UtcNow.AddDays(-1); });
        var manager = new ItemManager(context, null!, null!, null!);

        await manager.MarkItemReservedAsync(1, false);

        var item = await context.Items.FindAsync(1);
        Assert.False(item!.Reserved);
        Assert.Null(item.ReservedAt);
    }

    [Fact]
    public async Task GetItemsAwaitingSaleConfirmation_ReturnsOnlyOldReservations()
    {
        await using var context = CreateContext();
        // old reservation (8 days) → should appear
        NewItem(context, sellerId: 1, i =>
        {
            i.Reserved = true;
            i.ReservedAt = DateTime.UtcNow.AddDays(-8);
        });
        // fresh reservation (1 day) → too young
        NewItem(context, sellerId: 1, i =>
        {
            i.Reserved = true;
            i.ReservedAt = DateTime.UtcNow.AddDays(-1);
        });
        // old but already sold → decided
        NewItem(context, sellerId: 1, i =>
        {
            i.Reserved = false;
            i.ReservedAt = DateTime.UtcNow.AddDays(-20);
            i.IsSold = true;
            i.SoldAt = DateTime.UtcNow.AddDays(-10);
        });
        var manager = new ItemManager(context, null!, null!, null!);

        var awaiting = await manager.GetItemsAwaitingSaleConfirmationAsync(sellerId: 1);

        var item = Assert.Single(awaiting);
        Assert.Equal(1, item.Id);
    }

    [Fact]
    public async Task MarkItemSoldAsync_ConfirmsSaleAndEnablesRatingGate()
    {
        await using var context = CreateContext();
        NewItem(context, sellerId: 1, i =>
        {
            i.Reserved = true;
            i.ReservedAt = DateTime.UtcNow.AddDays(-8);
        });
        var manager = new ItemManager(context, null!, null!, null!);

        await manager.MarkItemSoldAsync(1);

        var item = await context.Items.FindAsync(1);
        Assert.True(item!.IsSold);
        Assert.NotNull(item.SoldAt);
        Assert.False(item.Reserved);
    }

    [Fact]
    public async Task MarkItemNotSoldAsync_ClosesCycleWithoutSale()
    {
        await using var context = CreateContext();
        NewItem(context, sellerId: 1, i =>
        {
            i.Reserved = true;
            i.ReservedAt = DateTime.UtcNow.AddDays(-8);
        });
        var manager = new ItemManager(context, null!, null!, null!);

        await manager.MarkItemNotSoldAsync(1);

        var item = await context.Items.FindAsync(1);
        Assert.False(item!.IsSold);
        Assert.False(item.Reserved);
        Assert.Null(item.ReservedAt);
    }

    [Fact]
    public async Task AutoCloseStaleReservationsAsync_MarksOnly30DayOldAsSold()
    {
        await using var context = CreateContext();
        // 31 days stale → auto-close
        NewItem(context, sellerId: 1, i =>
        {
            i.Reserved = true;
            i.ReservedAt = DateTime.UtcNow.AddDays(-31);
        });
        // 8 days → still within the decision window
        NewItem(context, sellerId: 1, i =>
        {
            i.Reserved = true;
            i.ReservedAt = DateTime.UtcNow.AddDays(-8);
        });
        var manager = new ItemManager(context, null!, null!, null!);

        var closed = await manager.AutoCloseStaleReservationsAsync();

        Assert.Equal(1, closed);
        var stale = await context.Items.FindAsync(1);
        Assert.True(stale!.IsSold);
        var young = await context.Items.FindAsync(2);
        Assert.False(young!.IsSold);
    }

    [Fact]
    public async Task AutoCloseStaleReservationsAsync_SkipsUnreservedItemWithStaleReservedAt()
    {
        // Invariant guard: ReservedAt without Reserved (broken state, e.g. after a
        // partial clear) must never be auto-sold.
        await using var context = CreateContext();
        NewItem(context, sellerId: 1, i =>
        {
            i.Reserved = false;
            i.ReservedAt = DateTime.UtcNow.AddDays(-60);
        });
        var manager = new ItemManager(context, null!, null!, null!);

        var closed = await manager.AutoCloseStaleReservationsAsync();

        Assert.Equal(0, closed);
        var item = await context.Items.FindAsync(1);
        Assert.False(item!.IsSold);
    }

    [Fact]
    public async Task RatingGate_FullFlow_SellerConfirmsThenBuyerCanRate()
    {
        await using var context = CreateContext();
        var item = NewItem(context, sellerId: 1, i =>
        {
            i.Reserved = true;
            i.ReservedAt = DateTime.UtcNow.AddDays(-8);
        });
        context.ChatThreads.Add(new ChatThread
        {
            Id = 1, ChannelId = "1-2", UserAId = 1, UserBId = 2, ItemId = item.Id,
        });
        context.SaveChanges();

        var items = new ItemManager(context, null!, null!, null!);
        var ratings = new RatingManager(context);

        // Before confirmation: no rating
        Assert.False(await ratings.CanRateAsync(2, 1));

        await items.MarkItemSoldAsync(item.Id);

        // After confirmation: buyer can rate the seller
        Assert.True(await ratings.CanRateAsync(2, 1));
        var result = await ratings.AddRatingAsync(2, 1, 5, "Great");
        Assert.True(result.Success);
    }
}
