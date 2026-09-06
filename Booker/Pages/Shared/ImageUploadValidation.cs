using Booker.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Booker.Pages.Shared;

public sealed record ValidatedImageBatch(List<Stream> Streams, List<string> Extensions);

public static class ImageUploadValidation
{
    public static async Task<ValidatedImageBatch?> ValidateAndReadAsync(
        List<IFormFile>? images,
        bool requireAtLeastOne,
        ModelStateDictionary modelState,
        string modelKey = "Input.Images")
    {
        var imageStreams = new List<Stream>();
        var imageExtensions = new List<string>();

        if (images == null || images.Count == 0)
        {
            if (requireAtLeastOne)
            {
                modelState.AddModelError(modelKey, "Proszę przesłać przynajmniej jedno zdjęcie książki.");
            }

            return modelState.IsValid ? new ValidatedImageBatch(imageStreams, imageExtensions) : null;
        }

        if (images.Count > ItemInputModel.MaxImageCount)
        {
            modelState.AddModelError(modelKey, $"Możesz przesłać maksymalnie {ItemInputModel.MaxImageCount} zdjęć.");
        }

        foreach (var image in images)
        {
            if (!image.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                modelState.AddModelError(modelKey, $"Plik {image.FileName} nie jest obrazem.");
                continue;
            }

            if (image.Length <= 0)
            {
                modelState.AddModelError(modelKey, $"Plik {image.FileName} jest pusty.");
                continue;
            }

            if (image.Length > ItemInputModel.MaxImageSizeBytes)
            {
                modelState.AddModelError(modelKey, $"Plik {image.FileName} przekracza limit {ItemInputModel.MaxImageSizeMb} MB.");
                continue;
            }

            var extension = Path.GetExtension(image.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !ItemInputModel.AllowedImageExtensions.Contains(extension))
            {
                modelState.AddModelError(
                    modelKey,
                    $"Plik {image.FileName} ma niedozwolone rozszerzenie. Dozwolone: {string.Join(", ", ItemInputModel.AllowedImageExtensions)}.");
                continue;
            }

            var detectedExtension = DetectImageExtension(image);
            if (detectedExtension == null)
            {
                modelState.AddModelError(modelKey, $"Plik {image.FileName} nie jest prawidłowym obrazem.");
                continue;
            }

            var memoryStream = new MemoryStream();
            await using var stream = image.OpenReadStream();
            await stream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            imageStreams.Add(memoryStream);
            imageExtensions.Add(detectedExtension);
        }

        if (!modelState.IsValid)
        {
            foreach (var stream in imageStreams)
            {
                stream.Dispose();
            }

            return null;
        }

        return new ValidatedImageBatch(imageStreams, imageExtensions);
    }

    /// <summary>
    /// Detects the real image format from magic bytes via ImageFormatDetector
    /// and returns the matching extension, so the stored filename and content
    /// type always match the actual content even when the upload is misnamed
    /// (e.g. PNG named .jpg). Returns null when the content is not a supported
    /// image.
    /// </summary>
    private static string? DetectImageExtension(IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            return ImageFormatDetector.DetectExtension(stream);
        }
        catch
        {
            return null;
        }
    }
}
