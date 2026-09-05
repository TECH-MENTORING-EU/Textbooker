using Booker.Data;
using Booker.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Booker.Tests;

/// <summary>
/// Unread message counting: a thread is unread for a participant when its
/// last message is newer than that participant's read stamp, and the
/// sender's own message never counts against the sender.
/// </summary>
public class ChatThreadUnreadTests
{
    private static readonly DateTime T0 = new(2026, 9, 5, 12, 0, 0, DateTimeKind.Utc);

    private static DataContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new DataContext(options);
        context.Users.AddRange(
            new User { Id = 1, UserName = "A", IsVisible = true },
            new User { Id = 2, UserName = "B", IsVisible = true });
        context.ChatThreads.Add(new ChatThread
        {
            Id = 1,
            ChannelId = "c1",
            UserAId = 1,
            UserBId = 2,
            CreatedUtc = T0,
            LastMessageUtc = T0,
        });
        context.SaveChanges();
        return context;
    }

    private static ChatThreadService NewService(DataContext context) =>
        new(context, null!, null!);

    [Fact]
    public async Task ThreadStartedByUserIsNotUnreadForThem()
    {
        await using var context = CreateContext();
        var svc = NewService(context);

        Assert.Equal(0, await svc.GetUnreadCountAsync(1, default));
        Assert.Equal(0, await svc.GetUnreadCountAsync(2, default));
    }

    [Fact]
    public async Task MessageNewerThanReadStampCountsAsUnread()
    {
        await using var context = CreateContext();
        var svc = NewService(context);

        // B writes one minute after creation; A never opened the thread.
        await svc.UpdateLastMessageUtcAsync("c1", senderId: 2, T0.AddMinutes(1), default);

        Assert.Equal(1, await svc.GetUnreadCountAsync(1, default));
        Assert.Equal(0, await svc.GetUnreadCountAsync(2, default));
    }

    [Fact]
    public async Task MarkThreadReadClearsTheCounter()
    {
        await using var context = CreateContext();
        var svc = NewService(context);
        await svc.UpdateLastMessageUtcAsync("c1", senderId: 2, T0.AddMinutes(1), default);

        await svc.MarkThreadReadAsync("c1", userId: 1, T0.AddMinutes(2), default);

        Assert.Equal(0, await svc.GetUnreadCountAsync(1, default));
    }

    [Fact]
    public async Task SendingCountsAsReadingForTheSender()
    {
        await using var context = CreateContext();
        var svc = NewService(context);

        await svc.UpdateLastMessageUtcAsync("c1", senderId: 1, T0.AddMinutes(1), default);

        Assert.Equal(0, await svc.GetUnreadCountAsync(1, default));
        // but the other participant sees it as unread
        Assert.Equal(1, await svc.GetUnreadCountAsync(2, default));
    }

    [Fact]
    public async Task ForeignUserIsNeverCounted()
    {
        await using var context = CreateContext();
        var svc = NewService(context);
        await svc.UpdateLastMessageUtcAsync("c1", senderId: 2, T0.AddMinutes(1), default);

        Assert.Equal(0, await svc.GetUnreadCountAsync(3, default));
    }
}
