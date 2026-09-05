using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.IO;
using System.Net;

namespace Booker.Services;

public sealed class PhotoStorageException : Exception
{
    public PhotoStorageException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

public class PhotosManager(ILogger<PhotosManager> logger, Lazy<IAmazonS3> s3Client, IConfiguration config)
{


    public async Task<string> AddPhotoAsync(Stream stream, string fileExtension)
    {
        var bucketName = config["S3:BucketName"];

        if (string.IsNullOrWhiteSpace(bucketName))
        {
            throw new PhotoStorageException(
                "Przesyłanie zdjęć jest tymczasowo niedostępne. Spróbuj ponownie później.");
        }

        var fileName = Guid.NewGuid().ToString() + fileExtension;

        var putRequest = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = fileName,
            InputStream = stream,
            CannedACL = S3CannedACL.PublicRead,
            ContentType = GetContentType(stream, fileExtension),
            UseChunkEncoding = false
        };

        PutObjectResponse response;
        try
        {
            response = await s3Client.Value.PutObjectAsync(putRequest);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Photo storage is not configured correctly.");
            throw new PhotoStorageException(
                "Przesyłanie zdjęć jest tymczasowo niedostępne. Spróbuj ponownie później.", ex);
        }
        catch (AmazonS3Exception ex)
        {
            logger.LogError(ex, "Photo upload to S3 failed.");
            throw new PhotoStorageException(
                "Nie można dodać zdjęcia. Spróbuj ponownie później albo skontaktuj się ze wsparciem.", ex);
        }

        if (response.HttpStatusCode == HttpStatusCode.OK)
        {
            return fileName;
        }
        else
        {
            logger.LogError("Failed to upload photo to S3. HTTP Status: {StatusCode}", response.HttpStatusCode);
            throw new PhotoStorageException(
                "Nie można dodać zdjęcia. Spróbuj ponownie później albo skontaktuj się ze wsparciem.");
        }
    }

    public async Task DeletePhotoAsync(string photoKey)
    {
        if (string.IsNullOrEmpty(photoKey)) return;

        var bucketName = config["S3:BucketName"];
        if (string.IsNullOrWhiteSpace(bucketName))
        {
            throw new PhotoStorageException("Photo storage bucket is not configured.");
        }

        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = bucketName,
            Key = photoKey
        };
        try
        {
            await s3Client.Value.DeleteObjectAsync(deleteRequest);
        }
        catch (InvalidOperationException ex)
        {
            throw new PhotoStorageException("Photo storage is not configured.", ex);
        }
        catch (AmazonS3Exception ex)
        {
            throw new PhotoStorageException($"Failed to delete photo object '{photoKey}' from S3.", ex);
        }
    }

    /// <summary>
    /// Deletes multiple photo objects and returns the keys that could not be deleted,
    /// so callers can report them for a later cleanup. Never throws.
    /// </summary>
    public async Task<List<string>> DeletePhotosAsync(IEnumerable<string>? photoKeys)
    {
        var failedKeys = new List<string>();

        foreach (var photoKey in photoKeys ?? Array.Empty<string>())
        {
            try
            {
                await DeletePhotoAsync(photoKey);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete photo object {PhotoKey} from storage.", photoKey);
                failedKeys.Add(photoKey);
            }
        }

        return failedKeys;
    }

    /// <summary>
    /// Splits a semicolon-separated photo list into storage keys. Root-relative values
    /// (starting with "/" or "\"), absolute URLs (starting with "http") and empty values
    /// are local assets or remote images, not storage objects, so they are skipped.
    /// Valid keys in this application are bare "<guid>.<ext>" object names.
    /// </summary>
    public static IEnumerable<string> StorageKeys(string? photoList)
    {
        return (photoList ?? "")
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(photo => photo.Trim())
            .Where(photo => photo.Length > 0
                && !photo.StartsWith('/')
                && !photo.StartsWith('\\')
                && !photo.StartsWith("http", StringComparison.OrdinalIgnoreCase));
    }

    public string GetPhotoUrl(string photoUri)
    {
        if (string.IsNullOrWhiteSpace(photoUri))
        {
            return string.Empty;
        }

        if (photoUri.StartsWith('/') || photoUri.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return photoUri;
        }

        var publicUrl = config["CF:PublicUrl"];
        return $"{publicUrl}/{photoUri}";
    }

    /// <summary>
    /// Determines the content type from the stream's magic bytes when the stream
    /// is seekable, so content that does not match its extension (e.g. PNG data
    /// stored with a .jpg extension) is not labeled with the wrong MIME type.
    /// Falls back to the file extension when the stream is not seekable or its
    /// format is not recognized.
    /// </summary>
    private static string GetContentType(Stream stream, string fileExtension)
    {
        if (stream.CanSeek)
        {
            var detectedExtension = ImageFormatDetector.DetectExtension(stream);
            if (detectedExtension != null)
            {
                return GetContentType(detectedExtension);
            }
        }

        return GetContentType(fileExtension);
    }

    private static string GetContentType(string fileExtension) => fileExtension.ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        _ => "application/octet-stream"
    };

}
