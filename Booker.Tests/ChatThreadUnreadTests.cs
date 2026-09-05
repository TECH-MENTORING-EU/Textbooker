using Booker.Data;
using Booker.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
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

    private static ChatService NewChatService(DataContext context) =>
        new(context, NullLogger<ChatService>.Instance,
            new ChatModerationService(Array.Empty<string>()));

    // Simulates an incoming message from user B straight on the thread row,
    // including the sender stamp the real send path applies; the send path
    // itself is covered by SendingCountsAsReadingForTheSender.
    private static void ReceiveMessage(DataContext context, DateTime at)
    {
        var thread = context.ChatThreads.Single(t => t.ChannelId == "c1");
        thread.LastMessageUtc = at;
        thread.UserBReadUtc = at; // the sender has seen their own message
        context.SaveChanges();
    }

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
        ReceiveMessage(context, T0.AddMinutes(1));

        Assert.Equal(1, await svc.GetUnreadCountAsync(1, default));
        Assert.Equal(0, await svc.GetUnreadCountAsync(2, default));
    }

    [Fact]
    public async Task MarkThreadReadClearsTheCounter()
    {
        await using var context = CreateContext();
        var svc = NewService(context);
        ReceiveMessage(context, T0.AddMinutes(1));

        await svc.MarkThreadReadAsync("c1", userId: 1, T0.AddMinutes(2), default);

        Assert.Equal(0, await svc.GetUnreadCountAsync(1, default));
    }

    [Fact]
    public async Task SendingCountsAsReadingForTheSender()
    {
        await using var context = CreateContext();
        var chat = NewChatService(context);

        var result = await chat.AddMessageAsync("c1", userId: 1, "hello", default);

        Assert.True(result.Success);
        var svc = NewService(context);
        // The sender has obviously seen their own message...
        Assert.Equal(0, await svc.GetUnreadCountAsync(1, default));
        // ...but the other participant sees it as unread.
        Assert.Equal(1, await svc.GetUnreadCountAsync(2, default));
    }

    [Fact]
    public async Task SendingUpdatesTheThreadStampInTheSameSave()
    {
        await using var context = CreateContext();
        var chat = NewChatService(context);

        var result = await chat.AddMessageAsync("c1", userId: 1, "hello", default);

        Assert.True(result.Success);
        var thread = await context.ChatThreads.SingleAsync(t => t.ChannelId == "c1");
        var message = await context.ChatMessages.SingleAsync(m => m.DealId == "c1");
        // One SaveChanges commits both: message and thread stamp agree.
        Assert.Equal(message.CreatedUtc, thread.LastMessageUtc);
    }

    [Fact]
    public async Task ForeignUserIsNeverCounted()
    {
        await using var context = CreateContext();
        var svc = NewService(context);
        ReceiveMessage(context, T0.AddMinutes(1));

        Assert.Equal(0, await svc.GetUnreadCountAsync(3, default));
    }
}
