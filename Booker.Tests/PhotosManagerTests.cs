using Amazon.S3;
using Booker.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Booker.Tests;

public class PhotosManagerTests
{
    [Fact]
    public void GetPhotoUrl_ForLocalAsset_DoesNotInitializeS3Client()
    {
        var s3WasInitialized = false;
        var s3Client = new Lazy<IAmazonS3>(() =>
        {
            s3WasInitialized = true;
            throw new InvalidOperationException("S3 should not be initialized while generating an URL.");
        });
        var configuration = new ConfigurationBuilder().Build();
        var manager = new PhotosManager(NullLogger<PhotosManager>.Instance, s3Client, configuration);

        var result = manager.GetPhotoUrl("/img/default-book.svg");

        Assert.Equal("/img/default-book.svg", result);
        Assert.False(s3WasInitialized);
    }

    [Fact]
    public void GetPhotoUrl_ForStoredKey_UsesConfiguredPublicUrlWithoutInitializingS3Client()
    {
        var s3WasInitialized = false;
        var s3Client = new Lazy<IAmazonS3>(() =>
        {
            s3WasInitialized = true;
            throw new InvalidOperationException("S3 should not be initialized while generating an URL.");
        });
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CF:PublicUrl"] = "https://cdn.example.test/"
            })
            .Build();
        var manager = new PhotosManager(NullLogger<PhotosManager>.Instance, s3Client, configuration);

        var result = manager.GetPhotoUrl("covers/book.jpg");

        Assert.Equal("https://cdn.example.test/covers/book.jpg", result);
        Assert.False(s3WasInitialized);
    }
}
