using Booker.Data;
using Booker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
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

            var validatedImages = await Shared.ImageUploadValidation.ValidateAndReadAsync(
                Input.Images,
                requireAtLeastOne: true,
                ModelState);

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("Input.Description",
                    "Opis wygląda na zawierający adres e-mail lub numer telefonu. Zaznacz potwierdzenie poniżej, jeśli mimo to chcesz opublikować ogłoszenie z taką treścią.");
                await LoadSelects(string.Empty);
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Page();
            }

            var parameters = await _staticDataManager.ConvertParametersAsync(
                Input.Title, Input.Grade, Input.Subject, Input.Level
            );

            var imageStreams = new List<Stream>();
            var imageExtensions = new List<string>();

            foreach (var img in Input.Images!)
            {

                var memoryStream = new MemoryStream();
                await img.OpenReadStream().CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                imageStreams.Add(memoryStream);

                imageExtensions.Add(Path.GetExtension(img.FileName));
            }


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
