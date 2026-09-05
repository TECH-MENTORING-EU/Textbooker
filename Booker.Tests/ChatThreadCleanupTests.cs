using Booker.Data;
using Booker.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Booker.Tests;

/// <summary>
/// Account deletion cleanup: threads point at users through non-cascading
/// foreign keys, so removing a user has to take their conversations - both
/// sides, with the messages - with it, while other users' threads survive.
/// </summary>
public class ChatThreadCleanupTests
{
    private static DataContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new DataContext(options);
        context.Users.AddRange(
            new User { Id = 1, UserName = "leaving", IsVisible = true },
            new User { Id = 2, UserName = "staying", IsVisible = true });
        // Thread 1: the leaving user talks with the staying one.
        // Thread 2: two other participants of the staying user's community.
        context.ChatThreads.AddRange(
            new ChatThread { Id = 1, ChannelId = "ch-deleted", UserAId = 1, UserBId = 2 },
            new ChatThread { Id = 2, ChannelId = "ch-kept", UserAId = 2, UserBId = 2 });
        context.ChatMessages.AddRange(
            new ChatMessage { DealId = "ch-deleted", UserId = 1, Content = "hi", CreatedUtc = DateTime.UtcNow },
            new ChatMessage { DealId = "ch-deleted", UserId = 2, Content = "hello", CreatedUtc = DateTime.UtcNow },
            new ChatMessage { DealId = "ch-kept", UserId = 2, Content = "still here", CreatedUtc = DateTime.UtcNow });
        context.SaveChanges();
        return context;
    }

    [Fact]
    public async Task DeletingAUserRemovesTheirThreadsAndTheirMessages()
    {
        await using var context = CreateContext();
        var svc = new ChatThreadService(context, null!, null!);

        await svc.DeleteThreadsForUserAsync(1, default);

        Assert.False(await context.ChatThreads.AnyAsync(t => t.ChannelId == "ch-deleted"));
        Assert.False(await context.ChatMessages.AnyAsync(m => m.DealId == "ch-deleted"));
    }

    [Fact]
    public async Task OtherUsersThreadsSurviveTheCleanup()
    {
        await using var context = CreateContext();
        var svc = new ChatThreadService(context, null!, null!);

        await svc.DeleteThreadsForUserAsync(1, default);

        Assert.True(await context.ChatThreads.AnyAsync(t => t.ChannelId == "ch-kept"));
        Assert.Equal(1, await context.ChatMessages.CountAsync(m => m.DealId == "ch-kept"));
    }

    [Fact]
    public async Task DeletingAUserWithoutThreadsChangesNothing()
    {
        await using var context = CreateContext();
        var svc = new ChatThreadService(context, null!, null!);

        await svc.DeleteThreadsForUserAsync(99, default);

        Assert.Equal(2, await context.ChatThreads.CountAsync());
        Assert.Equal(3, await context.ChatMessages.CountAsync());
    }
}
