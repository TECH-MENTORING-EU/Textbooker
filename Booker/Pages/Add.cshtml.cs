using Booker.Data;
using Booker.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;


namespace Booker.Pages
{
    public class BookAddingModel : Shared.BookFormModel<Shared.ItemAddModel>
    {
        public BookAddingModel(UserManager<User> userManager, StaticDataManager staticDataManager, ItemManager itemManager)
            : base(userManager, staticDataManager, itemManager)
        {
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var isUserAuthenticated = User.Identity?.IsAuthenticated ?? false;

            if (!isUserAuthenticated)
            {
                return Redirect("/Identity/Account/Login");
            }

            await LoadSelects();

            return Page();
        }        

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Redirect("/Identity/Account/Login");
            }

            if (Input == null)
            {
                ModelState.AddModelError(string.Empty, "Nieprawidłowe dane wejściowe. Proszę spróbować ponownie.");
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Page();
            }

            if (Input.Image == null || Input.Image.Length == 0)
            {
                ModelState.AddModelError("Input.Image", "Proszę przesłać zdjęcie książki.");
            }

            if (!ModelState.IsValid)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Page();
            }

            var parameters = await _staticDataManager.ConvertParametersAsync(
                Input.Title,
                Input.Grade,
                Input.Subject,
                Input.Level
            );

            using var stream = Input.Image!.OpenReadStream();

            var result = await _itemManager.AddItemAsync(new ItemManager.ItemModel(
                user,
                parameters,
                Input.Description,
                Input.State,
                Input.Price,
                stream,
                Path.GetExtension(Input.Image.FileName)
            ));

            return ValidateAndReturn(result.Id, result.Status);
        }
    }
}