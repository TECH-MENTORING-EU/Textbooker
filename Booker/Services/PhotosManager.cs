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

    public async Task DeletePhotoAsync(string photoUri)
    {
        if (string.IsNullOrEmpty(photoUri)) return;

        var bucketName = config["S3:BucketName"];

        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = bucketName,
            Key = photoUri
        };
        try
        {
            await s3Client.Value.DeleteObjectAsync(deleteRequest);
        }
        catch (Exception ex)
        {
            logger.LogWarning($"Error deleting old photo: {ex.Message}");
        }
    }
    public string GetPhotoUrl(string? photoUri, string? defaultUrl = null)
    {
        if (string.IsNullOrWhiteSpace(photoUri))
        {
            return defaultUrl ?? string.Empty;
        }

        if (IsTrustedPublicUrl(photoUri))
        {
            return photoUri;
        }

        // Anything that is not a bare storage key (foreign absolute URL,
        // network-path reference, inline scheme) must not reach an img src,
        // not even mangled onto the CDN base URL.
        if (photoUri.StartsWith('/') || photoUri.StartsWith('\\')
            || Uri.TryCreate(photoUri, UriKind.Absolute, out _))
        {
            return defaultUrl ?? string.Empty;
        }

        var publicUrl = config["CF:PublicUrl"];
        return $"{publicUrl}/{photoUri}";
    }

    private bool IsTrustedPublicUrl(string photoUri)
    {
        if (photoUri.StartsWith('/'))
        {
            // Root-relative application assets are same-origin; a second
            // slash or backslash makes a network-path reference the browser
            // resolves against a foreign host.
            return photoUri.Length == 1 || (photoUri[1] != '/' && photoUri[1] != '\\');
        }

        if (!Uri.TryCreate(photoUri, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return false;
        }

        return Uri.TryCreate(config["CF:PublicUrl"], UriKind.Absolute, out var publicBase)
            && uri.Host == publicBase.Host
            && uri.Port == publicBase.Port;
    }
}
