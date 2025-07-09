using System;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Booker.Services;

public class PhotosManager
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly IConfiguration _config;

    public PhotosManager(BlobServiceClient blobServiceClient, IConfiguration config)
    {
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



}
