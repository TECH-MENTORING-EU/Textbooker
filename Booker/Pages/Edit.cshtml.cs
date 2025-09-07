using Booker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Booker.Services;
using Microsoft.AspNetCore.Authorization;
using Booker.Authorization;

namespace Booker.Pages
{
    [Authorize]
    public class EditModel : Shared.BookFormModel<Shared.ItemEditModel>
    {
        public readonly IAuthorizationService _authService;
        public EditModel(UserManager<User> userManager, StaticDataManager staticDataManager, ItemManager itemManager, IAuthorizationService authService)
            : base(userManager, staticDataManager, itemManager)
        {
            _authService = authService;
        }

        public Item? ItemToEdit { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            ItemToEdit = await _itemManager.GetItemAsync(id);
            if (ItemToEdit == null) return NotFound();

            var isAuthorized = await _authService.AuthorizeAsync(User, ItemToEdit, ItemOperations.Update);

            if (!isAuthorized.Succeeded) return Forbid();

            Input = new Shared.ItemEditModel
            {
                Title = ItemToEdit.Book.Title,
                Subject = ItemToEdit.Book.Subject.Name,
                Grade = string.Join(',', ItemToEdit.Book.Grades.Select(g => g.GradeNumber).OrderBy(g => g)),
                Level = ItemToEdit.Book.Level.Name,
                Description = ItemToEdit.Description,
                State = ItemToEdit.State,
                Price = ItemToEdit.Price,
                Images = new List<IFormFile>(), // multiple images handled
                Reserved = ItemToEdit.Reserved
            };

            await LoadSelects(string.Empty);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
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
            if (ItemToEdit == null) return NotFound();

            var isAuthorized = await _authService.AuthorizeAsync(User, ItemToEdit, ItemOperations.Update);

            if (!isAuthorized.Succeeded) return Forbid();

            var parameters = await _staticDataManager.ConvertParametersAsync(
                Input.Title,
                Input.Grade,
                Input.Subject,
                Input.Level
            );

            if(Input.Reserved != ItemToEdit.Reserved)
            {
                await _itemManager.MarkItemReservedAsync(id, Input.Reserved);
            }

            var imageStreams = Input.Images?.Select(f => f.OpenReadStream()).ToList();
            var imageExtensions = Input.Images?.Select(f => Path.GetExtension(f.FileName)).ToList();

            var result = await _itemManager.UpdateItemAsync(ItemToEdit, new ItemManager.ItemModel(
                ItemToEdit.User,
                parameters,
                Input.Description,
                Input.State,
                Input.Price,
                imageStreams,
                imageExtensions,
                ItemToEdit.Photo
            ));

            return ValidateAndReturn(ItemToEdit.Id, result);
        }

        public async Task<IActionResult> OnPostDeleteAsync(int itemId)
        {
            var itemToDelete = await _itemManager.GetItemAsync(itemId);
            if (itemToDelete == null) return NotFound();
            
            var isAuthorized = await _authService.AuthorizeAsync(User, ItemToEdit, ItemOperations.Delete);
            if (!isAuthorized.Succeeded) return Forbid();

            await _itemManager.DeleteItemAsync(itemId);
            return RedirectToPage("/Index");
        }
    }
}
