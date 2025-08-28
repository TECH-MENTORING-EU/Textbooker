using Booker.Data;
using Booker.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System.IO;

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
                return Redirect("/Identity/Account/Login");

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

            if (!ModelState.IsValid)
            {
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
                user,
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
