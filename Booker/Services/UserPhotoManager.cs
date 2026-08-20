using Booker.Data;
using Microsoft.EntityFrameworkCore;

namespace Booker.Services;

/// <summary>
/// Keeps a user's photo objects in storage (R2) in sync with account deletion.
/// </summary>
public class UserPhotoManager(DataContext context, PhotosManager photosManager, ILogger<UserPhotoManager> logger)
{
    /// <summary>
    /// Collects every storage key the account owns: the profile picture and all photos
    /// of the user's items. Must run BEFORE the account row is deleted - the item rows
    /// cascade away with the account, and the keys can no longer be read afterwards.
    /// </summary>
    public async Task<List<string>> CollectPhotoKeysAsync(User user)
    {
        var itemPhotos = await context.Items
            .Where(i => i.UserId == user.Id)
            .Select(i => i.Photo)
            .ToListAsync();

        var keys = new List<string>();
        foreach (var photos in itemPhotos)
        {
            keys.AddRange(StorageKeys(photos));
        }

        keys.AddRange(StorageKeys(user.Photo));
        return keys;
    }

    /// <summary>
    /// Deletes the keys collected by <see cref="CollectPhotoKeysAsync"/> after the account
    /// was deleted. Storage failures never fail the request: the surviving keys are logged
    /// as orphaned so the objects can be purged later.
    /// </summary>
    public async Task DeleteFromStorageAsync(int userId, IReadOnlyCollection<string> photoKeys)
    {
        if (photoKeys.Count == 0) return;

        var orphanedKeys = await photosManager.DeletePhotosAsync(photoKeys);
        if (orphanedKeys.Count > 0)
        {
            logger.LogError(
                "Account {UserId} was deleted but {OrphanedCount} photo objects remain in storage. Orphaned keys: {OrphanedKeys}",
                userId, orphanedKeys.Count, string.Join(", ", orphanedKeys));
            return;
        }

        logger.LogInformation("Deleted {PhotoCount} photo objects of account {UserId} from storage.",
            photoKeys.Count, userId);
    }

    /// <summary>
    /// Item photos are stored as one semicolon-separated list, the profile picture as a
    /// single value. Root-relative values such as "/img/default-profile-picture.jpg" are
    /// local application assets, not storage objects, so they are skipped.
    /// </summary>
    private static IEnumerable<string> StorageKeys(string? photoList)
    {
        return (photoList ?? "")
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(photo => photo.Trim())
            .Where(photo => photo.Length > 0
                && !photo.StartsWith('/')
                && !photo.StartsWith("http", StringComparison.OrdinalIgnoreCase));
    }
}
