using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.IO;
using System.Net;

namespace Booker.Services;

public class PhotosManager
{
    private readonly ILogger<PhotosManager> _logger;
    private readonly IAmazonS3 _s3Client;
    private readonly IConfiguration _config;

    public PhotosManager(ILogger<PhotosManager> logger, IAmazonS3 s3Client, IConfiguration config)
    {
        _logger = logger;
        _s3Client = s3Client;
        _config = config;
    }

    public async Task<string> AddPhotoAsync(Stream stream, string fileExtension)
    {
        var bucketName = _config["S3:BucketName"];

        var fileName = Guid.NewGuid().ToString() + fileExtension;

        var putRequest = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = fileName,
            InputStream = stream,
            CannedACL = S3CannedACL.PublicRead,
            UseChunkEncoding = false
        };

        var response = await _s3Client.PutObjectAsync(putRequest);

        if (response.HttpStatusCode == HttpStatusCode.OK)
        {
            return fileName;
        }
        else
        {
            _logger.LogError("Failed to upload photo to S3. HTTP Status: {StatusCode}", response.HttpStatusCode);
            throw new Exception("Nie można dodać zdjęcia. Spróbuj ponownie później albo skontaktuj się z wsparciem.");
        }
    }

    public async Task DeletePhotoAsync(string photoUri)
    {
        if (string.IsNullOrEmpty(photoUri)) return;

        var bucketName = _config["S3:BucketName"];

        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = bucketName,
            Key = photoUri
        };
        try
        {
            await _s3Client.DeleteObjectAsync(deleteRequest);
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error deleting old photo: {ex.Message}");
        }
    }
    public string GetPhotoUrl(string photoUri)
    {
        var publicUrl = _config["CF:PublicUrl"];
        return $"{publicUrl}/{photoUri}";
    }

}
