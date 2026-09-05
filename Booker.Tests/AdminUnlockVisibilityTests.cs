using Booker.Data;
using Booker.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Booker.Tests;

/// <summary>
/// Admin block/unlock must restore exactly the state it changed: items hidden
/// BY THE BLOCK come back, items the user had hidden BEFORE the block stay hidden.
/// </summary>
public class AdminUnlockVisibilityTests : IDisposable
{
    private readonly DataContext _context;

    public AdminUnlockVisibilityTests()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new DataContext(options);

        var book = new Book
        {
            Id = 1, Title = "T", Grades = new List<Grade>(),
            Subject = new Subject { Id = 1, Name = "M" },
            Level = new Level { Id = 1, Name = "B" },
        };
        var user = new User { Id = 1, UserName = "blocked-user", IsVisible = true };
        _context.Users.Add(user);
        _context.Items.AddRange(
            new Item
            {
                Id = 1, UserId = 1, User = user, Book = book, Price = 30m, CreatedAt = DateTime.UtcNow,
                Description = "", State = "Good", Photo = "",
                IsVisible = true, CanChangeVisibility = true, // will be hidden by block
            },
            new Item
            {
                Id = 2, UserId = 1, User = user, Book = book, Price = 40m, CreatedAt = DateTime.UtcNow,
                Description = "", State = "Good", Photo = "",
                IsVisible = false, CanChangeVisibility = true, // user-hidden BEFORE block
            });
        _context.SaveChanges();
    }

    public void Dispose() => _context.Database.EnsureDeleted();

    [Fact]
    public async Task BlockHidesVisibleItemsAndMarksThemAdminControlled()
    {
        var manager = new ItemManager(_context, null!, null!, null!);

        await manager.SetItemsVisibilityByUserAsync(1, isVisible: false);

        var items = _context.Items.Local.OrderBy(i => i.Id).ToList();
        Assert.False(items[0].IsVisible);
        Assert.False(items[0].CanChangeVisibility); // marked: restore on unlock
        Assert.False(items[1].IsVisible);           // already hidden, stays hidden
        Assert.True(items[1].CanChangeVisibility);  // NOT marked - user hid it
    }

    [Fact]
    public async Task UnlockRepublishesOnlyBlockedHiddenItems()
    {
        var manager = new ItemManager(_context, null!, null!, null!);
        await manager.SetItemsVisibilityByUserAsync(1, isVisible: false);

        await manager.SetItemsVisibilityByUserAsync(1, isVisible: true);

        var item1 = await _context.Items.FindAsync(1);
        var item2 = await _context.Items.FindAsync(2);

        // Admin-controlled item is restored...
        Assert.True(item1!.IsVisible);
        Assert.True(item1.CanChangeVisibility);
        // ...but the user's own pre-block hidden offer stays hidden.
        Assert.False(item2!.IsVisible);
        Assert.True(item2.CanChangeVisibility);
    }

    [Fact]
    public async Task DoubleBlockDoubleUnlockDoesNotCorruptState()
    {
        var manager = new ItemManager(_context, null!, null!, null!);

        await manager.SetItemsVisibilityByUserAsync(1, isVisible: false);
        await manager.SetItemsVisibilityByUserAsync(1, isVisible: false); // no visible items left, no-op
        await manager.SetItemsVisibilityByUserAsync(1, isVisible: true);
        await manager.SetItemsVisibilityByUserAsync(1, isVisible: true); // nothing left to restore, no-op

        var item1 = await _context.Items.FindAsync(1);
        var item2 = await _context.Items.FindAsync(2);

        Assert.True(item1!.IsVisible);
        Assert.False(item2!.IsVisible); // user-hidden, still hidden
    }
}
