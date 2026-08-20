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
    /// format is not recognized. The stream position is restored after reading.
    /// </summary>
    public static string GetContentType(Stream stream, string fileExtension)
    {
        if (stream.CanSeek)
        {
            var detectedContentType = DetectContentType(stream);
            if (detectedContentType != null)
            {
                return detectedContentType;
            }
        }

        return GetContentType(fileExtension);
    }

    private static string? DetectContentType(Stream stream)
    {
        var originalPosition = stream.Position;
        try
        {
            var header = new byte[8];
            var bytesRead = stream.Read(header, 0, header.Length);

            // JPEG: FF D8, matching ImageUploadValidation.DetectImageExtension.
            if (bytesRead >= 2 && header[0] == 0xFF && header[1] == 0xD8)
            {
                return "image/jpeg";
            }

            // PNG: 89 50 4E 47 0D 0A 1A 0A.
            if (bytesRead >= 8 &&
                header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
            {
                return "image/png";
            }

            return null;
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }

    public static string GetContentType(string fileExtension) => fileExtension.ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        _ => "application/octet-stream"
    };

}
