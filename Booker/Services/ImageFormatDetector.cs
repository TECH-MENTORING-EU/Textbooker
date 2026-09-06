using System.IO;

namespace Booker.Services;

/// <summary>
/// Single source of truth for image magic-byte signatures, shared by upload
/// validation and S3 content-type labeling so the two cannot drift apart.
/// Returns the canonical extension for the detected format, or null when the
/// content is not a supported image.
/// </summary>
public static class ImageFormatDetector
{
    public static string? DetectExtension(Stream stream)
    {
        var originalPosition = stream.Position;
        try
        {
            var header = new byte[8];
            var bytesRead = stream.Read(header, 0, header.Length);

            if (bytesRead < 2)
            {
                return null;
            }

            // JPEG: FF D8.
            if (header[0] == 0xFF && header[1] == 0xD8)
            {
                return ".jpg";
            }

            // PNG: 89 50 4E 47 0D 0A 1A 0A.
            if (bytesRead >= 8 &&
                header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
            {
                return ".png";
            }

            return null;
        }
        finally
        {
            if (stream.CanSeek)
            {
                stream.Position = originalPosition;
            }
        }
    }
}
