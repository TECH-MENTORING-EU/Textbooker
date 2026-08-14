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

            if (!IsValidImageSignature(image))
            {
                modelState.AddModelError(modelKey, $"Plik {image.FileName} nie jest prawidłowym obrazem.");
                continue;
            }

            var memoryStream = new MemoryStream();
            await using var stream = image.OpenReadStream();
            await stream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            imageStreams.Add(memoryStream);
            imageExtensions.Add(extension);
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

    private static bool IsValidImageSignature(IFormFile file)
    {
        try
        {
            var header = new byte[8];
            using var stream = file.OpenReadStream();
            var bytesRead = stream.Read(header, 0, header.Length);

            if (bytesRead < 2)
            {
                return false;
            }

            // JPEG
            if (header[0] == 0xFF && header[1] == 0xD8)
            {
                return true;
            }

            // PNG
            if (bytesRead >= 8 &&
                header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
            {
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}
