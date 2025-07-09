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

        var fileName = Guid.NewGuid().ToString() + fileExtension;
        var blobClient = containerClient.GetBlobClient(fileName);

        await blobClient.UploadAsync(stream, overwrite: true);

        return blobClient.Uri;
    }

    public async Task DeletePhotoAsync(string photoUri)
    {
        if (string.IsNullOrEmpty(photoUri)) return;

        var containerName = _config["AzureStorage:ContainerName"];
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        try
        {
            var oldBlobName = Path.GetFileName(new Uri(photoUri).LocalPath);
            var oldBlobClient = containerClient.GetBlobClient(oldBlobName);
            await oldBlobClient.DeleteIfExistsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error deleting old photo: {ex.Message}");
        }
    }


}
