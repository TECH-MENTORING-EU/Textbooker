using Booker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Booker.Services;

namespace Booker.Pages
{
    public class EditModel : Shared.BookFormModel<Shared.ItemEditModel>
    {
        public EditModel(UserManager<User> userManager, StaticDataManager staticDataManager, ItemManager itemManager)
            : base(userManager, staticDataManager, itemManager)
        {
        }

        public Item? ItemToEdit { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Redirect("/Identity/Account/Login");
            }

            ItemToEdit = await _itemManager.GetItemAsync(id);

            if (ItemToEdit == null)
            {
                return NotFound();
            }

            if (ItemToEdit.User.Id != user.Id)
            {
                return Forbid();
            }

            Input = new Shared.ItemEditModel
            {
                Title = ItemToEdit.Book.Title,
                Subject = ItemToEdit.Book.Subject.Name,
                Grade = ItemToEdit.Book.Grades.First().GradeNumber,
                Level = ItemToEdit.Book.Level == true ? "Rozszerzenie" : "Podstawa",
                Description = ItemToEdit.Description,
                State = ItemToEdit.State,
                Price = ItemToEdit.Price,
                Image = null!
            };

            await LoadSelects();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
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

            if (!ModelState.IsValid)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Page();
            }            

            ItemToEdit = await _itemManager.GetItemAsync(id);

            if (ItemToEdit == null)
            {
                return NotFound();
            }

            if (ItemToEdit.User.Id != user.Id)
            {
                return Forbid();
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

            using var stream = Input.Image?.OpenReadStream();


            var result = await _itemManager.UpdateItemAsync(ItemToEdit, new ItemManager.ItemModel(
                user,
                parameters,
                Input.Description,
                Input.State,
                Input.Price,
                stream,
                Path.GetExtension(Input.Image?.FileName),
                ItemToEdit.Photo
            ));

            return ValidateAndReturn(ItemToEdit.Id, result);
        }

        public async Task<IActionResult> OnPostDeleteAsync(int itemId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Redirect("/Identity/Account/Login");
            }

            var itemToDelete = await _itemManager.GetItemAsync(itemId);

            if (itemToDelete == null)
            {
                return NotFound();
            }

            if (itemToDelete.User.Id != user.Id)
            {
                return Forbid();
            }

            await _itemManager.DeleteItemAsync(itemId);

            return RedirectToPage("/Index");
        }
    }
}