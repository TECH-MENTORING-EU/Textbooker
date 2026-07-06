using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.IO;
using System.Net;

namespace Booker.Services;

public class PhotosManager(ILogger<PhotosManager> logger, Lazy<IAmazonS3> s3Client, IConfiguration config)
{


    public async Task<string> AddPhotoAsync(Stream stream, string fileExtension)
    {
        var bucketName = config["S3:BucketName"];

        var fileName = Guid.NewGuid().ToString() + fileExtension;

        var putRequest = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = fileName,
            InputStream = stream,
            CannedACL = S3CannedACL.PublicRead,
            UseChunkEncoding = false
        };

        var response = await s3Client.Value.PutObjectAsync(putRequest);

        if (response.HttpStatusCode == HttpStatusCode.OK)
        {
            return fileName;
        }
        else
        {
            logger.LogError("Failed to upload photo to S3. HTTP Status: {StatusCode}", response.HttpStatusCode);
            throw new Exception("Nie można dodać zdjęcia. Spróbuj ponownie później albo skontaktuj się z wsparciem.");
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

        if (photoUri.StartsWith('/'))
        {
            return photoUri;
        }

        var publicUrl = config["CF:PublicUrl"]?.TrimEnd('/');

        if (Uri.TryCreate(photoUri, UriKind.Absolute, out var absoluteUri))
        {
            if (!IsTrustedPublicUrl(absoluteUri, publicUrl))
            {
                logger.LogWarning("Rejected an untrusted absolute photo URL for host {Host}.", absoluteUri.Host);
                return string.Empty;
            }

            return absoluteUri.ToString();
        }

        if (string.IsNullOrWhiteSpace(publicUrl))
        {
            return photoUri.StartsWith('/') ? photoUri : $"/{photoUri}";
        }

        return $"{publicUrl}/{photoUri.TrimStart('/')}";
    }

    private static bool IsTrustedPublicUrl(Uri candidate, string? configuredPublicUrl)
    {
        return Uri.TryCreate(configuredPublicUrl, UriKind.Absolute, out var configuredUri) &&
            (candidate.Scheme == Uri.UriSchemeHttp || candidate.Scheme == Uri.UriSchemeHttps) &&
            string.Equals(candidate.Host, configuredUri.Host, StringComparison.OrdinalIgnoreCase) &&
            candidate.Port == configuredUri.Port;
    }

}
