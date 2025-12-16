using Booker.Data;
using Booker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Booker.Pages
{
    [Authorize]
    public class BookAddingModel : Shared.BookFormModel<Shared.ItemAddModel>
    {
        public BookAddingModel(UserManager<User> userManager, StaticDataManager staticDataManager, ItemManager itemManager)
            : base(userManager, staticDataManager, itemManager)
        {
        }

        // Magic byte
        private static bool IsValidImageSignature(IFormFile file)
        {
            byte[] header = new byte[8];
            using var stream = file.OpenReadStream();
            int bytesRead = stream.Read(header, 0, header.Length);
            if (bytesRead < 2) return false;

            // JPEG
            if (header[0] == 0xFF && header[1] == 0xD8)
                return true;

            // PNG
            if (bytesRead >= 8 &&
                header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
                return true;

            return true;
        }


        public async Task<IActionResult> OnGetAsync()
        {
            await LoadSelects(string.Empty);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Input == null)
            {
                ModelState.AddModelError(string.Empty, "Nieprawidłowe dane wejściowe. Proszę spróbować ponownie.");
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Page();
            }

            if (Input.Images == null || Input.Images.Count == 0)
                ModelState.AddModelError("Input.Images", "Proszę przesłać przynajmniej jedno zdjęcie książki.");
            else if (Input.Images.Count > 6)
                ModelState.AddModelError("Input.Images", "Możesz przesłać maksymalnie 6 zdjęć.");

            var imageStreams = new List<Stream>();
            var imageExtensions = new List<string>();

            foreach (var img in Input.Images!)
            {
                if (!img.ContentType.StartsWith("image/"))
                {
                    ModelState.AddModelError("Input.Images", $"Plik {img.FileName} nie jest obrazem.");
                    continue;
                }

                // Optional: check extensions for UX
                string ext = Path.GetExtension(img.FileName)?.ToLowerInvariant();
                if (string.IsNullOrEmpty(ext))
                {
                    ModelState.AddModelError("Input.Images", $"Plik {img.FileName} nie ma rozszerzenia.");
                    continue;
                }

                if (!IsValidImageSignature(img))
                {
                    ModelState.AddModelError("Input.Images", $"Plik {img.FileName} nie jest prawidłowym obrazem.");
                    continue;
                }

                var ms = new MemoryStream();
                await img.CopyToAsync(ms);
                ms.Position = 0;

                imageStreams.Add(ms);
                imageExtensions.Add(ext);
            }


            if (!ModelState.IsValid)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Page();
            }

            var parameters = await _staticDataManager.ConvertParametersAsync(
                Input.Title, Input.Grade, Input.Subject, Input.Level
            );

            var result = await _itemManager.AddItemAsync(new ItemManager.ItemModel(
                (await _userManager.GetUserAsync(User))!,
                parameters,
                Input.Description,
                Input.State,
                Input.Price,
                imageStreams,
                imageExtensions
            ));

            return ValidateAndReturn(result.Id, result.Status);
        }
    }
}
