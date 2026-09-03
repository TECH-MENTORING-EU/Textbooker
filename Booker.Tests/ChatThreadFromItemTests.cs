using Booker.Data;
using Booker.Services;
using Booker.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Booker.Tests;

/// <summary>
/// Threads about listings: chat can only start from an offer page ("Napisz do
/// sprzedającego"), never user-to-user cold. One thread per (item, user pair).
/// </summary>
public class ChatThreadFromItemTests : IDisposable
{
    private readonly DataContext _context;
    private readonly ChatThreadService _service;

    public ChatThreadFromItemTests()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new DataContext(options);
        _service = new ChatThreadService(_context, NullLogger<ChatThreadService>.Instance);
    }

    public void Dispose() => _context.Database.EnsureDeleted();

    private (User seller, User buyer, Item item) SeedItem(int sellerId = 2, int buyerId = 1)
    {
        var book = new Book
        {
            Id = 1,
            Title = "Test Book",
            Grades = new List<Grade>(),
            Subject = new Subject { Id = 1, Name = "Math" },
            Level = new Level { Id = 1, Name = "Basic" },
        };
        var seller = new User { Id = sellerId, UserName = $"seller{sellerId}" };
        var buyer = new User { Id = buyerId, UserName = $"buyer{buyerId}" };
        var item = new Item
        {
            Id = 10,
            UserId = sellerId,
            User = seller,
            Book = book,
            Price = 25.99m,
            State = "Good",
            Photo = "test.jpg",
            Description = "Test",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.AddRange(seller, buyer);
        _context.Items.Add(item);
        _context.SaveChanges();
        return (seller, buyer, item);
    }

    [Fact]
    public async Task GetOrCreateForItemAsync_CreatesThreadLinkedToItem()
    {
        SeedItem();

        var thread = await _service.GetOrCreateForItemAsync(1, 10, CancellationToken.None);

        Assert.Equal(10, thread.ItemId);
        Assert.Equal(1, Math.Min(thread.UserAId, thread.UserBId));
        Assert.Equal(2, Math.Max(thread.UserAId, thread.UserBId));
        Assert.StartsWith("item-10-", thread.ChannelId);
    }

    [Fact]
    public async Task GetOrCreateForItemAsync_IsIdempotent_SamePairSameItem()
    {
        SeedItem();

        var t1 = await _service.GetOrCreateForItemAsync(1, 10, CancellationToken.None);
        var t2 = await _service.GetOrCreateForItemAsync(1, 10, CancellationToken.None);

        Assert.Equal(t1.ChannelId, t2.ChannelId);
        Assert.Equal(1, await _context.ChatThreads.CountAsync());
    }

    [Fact]
    public async Task GetOrCreateForItemAsync_DifferentBuyersGetDifferentThreads()
    {
        SeedItem(sellerId: 5, buyerId: 1);

        var t1 = await _service.GetOrCreateForItemAsync(1, 10, CancellationToken.None);
        var t2 = await _service.GetOrCreateForItemAsync(7, 10, CancellationToken.None);

        Assert.NotEqual(t1.ChannelId, t2.ChannelId);
        Assert.All(new[] { t1, t2 }, t => Assert.Equal(10, t.ItemId));
    }

    [Fact]
    public async Task GetOrCreateForItemAsync_SellerCannotChatAboutOwnListing()
    {
        SeedItem(sellerId: 2);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetOrCreateForItemAsync(2, 10, CancellationToken.None));
    }

    [Fact]
    public async Task GetOrCreateForItemAsync_NonexistentItem_Throws()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetOrCreateForItemAsync(1, 999, CancellationToken.None));
    }

    [Fact]
    public async Task GetOrCreateForItemAsync_ChannelIdNeverCollidesWithLegacyThreads()
    {
        SeedItem(sellerId: 2, buyerId: 1);
        // legacy bare thread for the same pair
        _context.ChatThreads.Add(new ChatThread
        {
            ChannelId = ThreadIdBuilder.Create(1, 2),
            UserAId = 1,
            UserBId = 2
        });
        await _context.SaveChangesAsync();

        var thread = await _service.GetOrCreateForItemAsync(1, 10, CancellationToken.None);

        Assert.NotEqual("1-2", thread.ChannelId);
        Assert.Equal(2, await _context.ChatThreads.CountAsync());
    }
}
