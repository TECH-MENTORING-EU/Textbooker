using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Booker.Services;
using Booker.TestUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Booker.Tests.Services;

/// <summary>
/// Characterization of the storage adapter: public-read uploads keyed by guid,
/// content type resolved from magic bytes rather than the claimed extension
/// (commit 8d76895), bare-key filtering for deletes, and a delete loop that
/// reports failures instead of throwing.
/// </summary>
public class PhotosManagerTests
{
    private static IConfiguration ConfigWithBucket() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["S3:BucketName"] = "test-bucket",
            ["CF:PublicUrl"] = "https://cdn.test",
        })
        .Build();

    private static PhotosManager Create(S3Recorder recorder, IConfiguration? config = null) => new(
        NullLogger<PhotosManager>.Instance,
        new Lazy<IAmazonS3>(recorder.BuildClient),
        config ?? ConfigWithBucket());

    [Fact]
    public async Task AddPhotoAsync_uploads_public_read_with_jpeg_content_type()
    {
        var recorder = new S3Recorder();
        var manager = Create(recorder);

        var key = await manager.AddPhotoAsync(new MemoryStream(TestImages.Jpeg), ".jpg");

        Assert.EndsWith(".jpg", key);
        var put = Assert.Single(recorder.Puts);
        Assert.Equal("test-bucket", put.BucketName);
        Assert.Equal(key, put.Key);
        Assert.Equal(S3CannedACL.PublicRead, put.CannedACL);
        Assert.Equal("image/jpeg", put.ContentType);
    }

    [Fact]
    public async Task AddPhotoAsync_labels_content_type_by_magic_bytes_not_extension()
    {
        // PNG data named .jpg: the stored key keeps the passed extension, but the
        // object must not be served as image/jpeg (commit 8d76895).
        var recorder = new S3Recorder();
        var manager = Create(recorder);

        var key = await manager.AddPhotoAsync(new MemoryStream(TestImages.Png), ".jpg");

        Assert.EndsWith(".jpg", key);
        Assert.Equal("image/png", Assert.Single(recorder.Puts).ContentType);
    }

    [Fact]
    public async Task AddPhotoAsync_throws_when_the_bucket_is_not_configured()
    {
        var manager = Create(new S3Recorder(), new ConfigurationBuilder().AddInMemoryCollection().Build());

        await Assert.ThrowsAsync<PhotoStorageException>(
            () => manager.AddPhotoAsync(new MemoryStream(TestImages.Jpeg), ".jpg"));
    }

    [Fact]
    public void StorageKeys_keeps_bare_keys_only()
    {
        var keys = PhotosManager.StorageKeys("k1;https://ext/img.png;k2.jpg;/root/asset;\\win\\path;  ;k3")
            .ToArray();

        Assert.Equal(["k1", "k2.jpg", "k3"], keys);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(";;")]
    public void StorageKeys_returns_empty_for_no_photos(string? photoList)
    {
        Assert.Empty(PhotosManager.StorageKeys(photoList));
    }

    [Fact]
    public void GetPhotoUrl_joins_the_public_url_with_a_single_slash()
    {
        var manager = Create(new S3Recorder());

        Assert.Equal("https://cdn.test/some-key.jpg", manager.GetPhotoUrl("some-key.jpg"));
    }

    [Theory]
    [InlineData("/local/asset.png")]
    [InlineData("https://elsewhere.example/img.png")]
    [InlineData("")]
    public void GetPhotoUrl_passes_through_urls_and_empty_values(string photoUri)
    {
        var manager = Create(new S3Recorder());

        Assert.Equal(photoUri, manager.GetPhotoUrl(photoUri));
    }

    [Fact]
    public async Task DeletePhotosAsync_deletes_each_key_and_reports_no_failures()
    {
        var recorder = new S3Recorder();
        var manager = Create(recorder);

        var failed = await manager.DeletePhotosAsync(["a.jpg", "b.png"]);

        Assert.Empty(failed);
        Assert.Equal(["a.jpg", "b.png"], recorder.Deletes.Select(d => d.Key));
    }

    [Fact]
    public async Task DeletePhotosAsync_returns_failed_keys_instead_of_throwing()
    {
        var recorder = new S3Recorder { FailDeletes = true };
        var manager = Create(recorder);

        var failed = await manager.DeletePhotosAsync(["a.jpg", "b.png"]);

        Assert.Equal(["a.jpg", "b.png"], failed);
    }

    [Fact]
    public async Task DeletePhotosAsync_skips_empty_keys()
    {
        var recorder = new S3Recorder();
        var manager = Create(recorder);

        var failed = await manager.DeletePhotosAsync(["", null!]);

        Assert.Empty(failed);
        Assert.Empty(recorder.Deletes);
    }
}
