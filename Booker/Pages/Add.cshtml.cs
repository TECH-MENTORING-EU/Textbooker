using Booker.Data;
using Booker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

            if (validatedImages == null)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                await LoadSelects(string.Empty);
                return Page();
            }

            var parameters = await _staticDataManager.ConvertParametersAsync(
                Input.Title, Input.Grade, Input.Subject, Input.Level
            );

            ItemManager.Result result;
            try
            {
                result = await _itemManager.AddItemAsync(new ItemManager.ItemModel(
                    (await _userManager.GetUserAsync(User))!,
                    parameters,
                    Input.Description ?? string.Empty,
                    Input.State,
                    Input.Price,
                    validatedImages.Streams,
                    validatedImages.Extensions
                ));
            }
            catch (PhotoStorageException ex)
            {
                ModelState.AddModelError("Input.Images", ex.Message);
                Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await LoadSelects(string.Empty);
                return Page();
            }

            return ValidateAndReturn(result.Id, result.Status);
        }
    }
}
