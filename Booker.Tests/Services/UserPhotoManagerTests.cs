using Booker.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Booker.Tests.Services;

/// <summary>
/// Characterization of the account-deletion photo purge ordering (GDPR, commit
/// 6ea5f1a): keys are collected while the rows still exist, external URLs are
/// never treated as storage objects, and storage failures are logged, not thrown.
/// </summary>
public class UserPhotoManagerTests
{
    /// <summary>One owner with a profile photo and two items (bare keys + external URLs).</summary>
    private static async Task<ItemManagerTestHost> SeedOwnerWithPhotosAsync()
    {
        var host = new ItemManagerTestHost();
        var userId = await host.SeedUserAsync("gdpr_owner");
        var user = await host.GetUserAsync(userId);
        user.Photo = "profile-key.png";
        await host.Context.SaveChangesAsync();

        await host.SeedItemAsync(userId, photo: "item-1.jpg;item-2.png");
        await host.SeedItemAsync(userId, photo: "https://cdn.test/external.png;/root/asset.png");
        return host;
    }

    [Fact]
    public async Task CollectPhotoKeysAsync_returns_item_keys_then_the_profile_key()
    {
        await using var host = await SeedOwnerWithPhotosAsync();
        var manager = new UserPhotoManager(host.Context, host.Photos, NullLogger<UserPhotoManager>.Instance);

        var keys = await manager.CollectPhotoKeysAsync(await TheOwnerAsync(host));

        Assert.Equal(["item-1.jpg", "item-2.png", "profile-key.png"], keys);
    }

    [Fact]
    public async Task DeleteFromStorageAsync_deletes_the_collected_keys()
    {
        await using var host = await SeedOwnerWithPhotosAsync();
        var manager = new UserPhotoManager(host.Context, host.Photos, NullLogger<UserPhotoManager>.Instance);
        var user = await TheOwnerAsync(host);

        var keys = await manager.CollectPhotoKeysAsync(user);
        await manager.DeleteFromStorageAsync(user.Id, keys);

        Assert.Equal(["item-1.jpg", "item-2.png", "profile-key.png"], host.S3.Deletes.Select(d => d.Key));
    }

    [Fact]
    public async Task DeleteFromStorageAsync_never_throws_during_a_storage_outage()
    {
        await using var host = await SeedOwnerWithPhotosAsync();
        host.S3.FailDeletes = true;
        var manager = new UserPhotoManager(host.Context, host.Photos, NullLogger<UserPhotoManager>.Instance);

        // Record the keys before deleting the account row, exactly as the admin
        // page handler does - the item rows cascade away with the account.
        var user = await TheOwnerAsync(host);
        var keys = await manager.CollectPhotoKeysAsync(user);
        host.Context.Users.Remove(user);
        await host.Context.SaveChangesAsync();

        await manager.DeleteFromStorageAsync(user.Id, keys); // must not throw
        Assert.Equal(3, host.S3.Deletes.Count); // the attempts were still made
    }

    [Fact]
    public async Task DeleteFromStorageAsync_with_no_keys_is_a_no_op()
    {
        await using var host = await SeedOwnerWithPhotosAsync();
        var manager = new UserPhotoManager(host.Context, host.Photos, NullLogger<UserPhotoManager>.Instance);
        var user = await TheOwnerAsync(host);

        await manager.DeleteFromStorageAsync(user.Id, []);

        Assert.Empty(host.S3.Deletes);
    }

    private static Task<Booker.Data.User> TheOwnerAsync(ItemManagerTestHost host) =>
        host.Context.Users.SingleAsync();
}
