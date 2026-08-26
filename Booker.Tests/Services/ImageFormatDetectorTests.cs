using System.Text;
using Booker.Services;

namespace Booker.Tests.Services;

/// <summary>
/// Characterization of the magic-byte table (commits 8b0e88b/8d76895): only JPEG
/// and PNG signatures are accepted, anything else is rejected so uploads cannot
/// smuggle arbitrary content labelled as an image.
/// </summary>
public class ImageFormatDetectorTests
{
    public static TheoryData<byte[], string?> Headers => new()
    {
        // JPEG: FF D8 is enough (plan/audit contract).
        { new byte[] { 0xFF, 0xD8 }, ".jpg" },
        { new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46 }, ".jpg" },
        // Full 8-byte PNG signature.
        { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00 }, ".png" },
        // Truncated PNG (first 4 bytes only) is NOT a PNG.
        { new byte[] { 0x89, 0x50, 0x4E, 0x47 }, null },
        // Text is not an image.
        { Encoding.UTF8.GetBytes("hello"), null },
        // Single arbitrary byte.
        { new byte[] { 0x00 }, null },
        // Empty stream.
        { Array.Empty<byte>(), null },
    };

    [Theory]
    [MemberData(nameof(Headers))]
    public void DetectExtension_matches_the_magic_byte_table(byte[] content, string? expected)
    {
        using var stream = new MemoryStream(content);

        Assert.Equal(expected, ImageFormatDetector.DetectExtension(stream));
    }

    [Fact]
    public void DetectExtension_restores_the_stream_position()
    {
        using var stream = new MemoryStream(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00 });

        Assert.Equal(".jpg", ImageFormatDetector.DetectExtension(stream));
        Assert.Equal(0, stream.Position);
    }

    [Fact]
    public void DetectExtension_reads_the_header_from_the_current_position()
    {
        var buffer = new byte[] { 0x00, 0x01, 0x02, 0x03, 0xFF, 0xD8, 0xFF, 0xE0 };
        using var stream = new MemoryStream(buffer);
        stream.Position = 4;

        Assert.Equal(".jpg", ImageFormatDetector.DetectExtension(stream));
        Assert.Equal(4, stream.Position); // restored to where the caller left it
    }
}
