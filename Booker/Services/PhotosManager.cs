using System;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Booker.Services;

public class PhotosManager
{
    private readonly ILogger<PhotosManager> _logger;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly IConfiguration _config;

    public PhotosManager(ILogger<PhotosManager> logger, BlobServiceClient blobServiceClient, IConfiguration config)
    {
        _logger = logger;
        _blobServiceClient = blobServiceClient;
        _config = config;
    }

    public async Task<Uri> AddPhotoAsync(Stream stream, string fileExtension)
    {
        var containerName = _config["AzureStorage:ContainerName"];
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        if (string.IsNullOrWhiteSpace(bucketName))
        {
            throw new PhotoStorageException(
                "Przesyłanie zdjęć jest tymczasowo niedostępne. Spróbuj ponownie później.");
        }

        var fileName = Guid.NewGuid().ToString() + fileExtension;
        var blobClient = containerClient.GetBlobClient(fileName);

        await blobClient.UploadAsync(stream, overwrite: true);

        return blobClient.Uri;
    }

    public async Task DeletePhotoAsync(string photoKey)
    {
        if (string.IsNullOrEmpty(photoKey)) return;

        var containerName = _config["AzureStorage:ContainerName"];
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        try
        {
            var oldBlobName = Path.GetFileName(new Uri(photoUri).LocalPath);
            var oldBlobClient = containerClient.GetBlobClient(oldBlobName);
            await oldBlobClient.DeleteIfExistsAsync();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Error deleting old photo: {ex.Message}");
        }
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
