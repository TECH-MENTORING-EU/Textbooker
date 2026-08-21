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
        private readonly IAuthorizationService _authService;
        private readonly ILogger<EditModel> _logger;

        public EditModel(UserManager<User> userManager, StaticDataManager staticDataManager, ItemManager itemManager, IAuthorizationService authService, ILogger<EditModel> logger)
            : base(userManager, staticDataManager, itemManager)
        {
            _authService = authService;
            _logger = logger;
        }

        public Item? ItemToEdit { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            ItemToEdit = await _itemManager.GetItemAsync(id);
            if (ItemToEdit == null) return NotFound();

            var isAuthorized = await _authService.AuthorizeAsync(User, ItemToEdit, ItemOperations.Update);

            if (!isAuthorized.Succeeded)
            {
                _logger.LogWarning($"Użytkownik {User.Identity?.Name} próbował wykonać nieuprawnioną akcję {ItemOperations.Update.Name} na zasobie o ID {id}.");
                return Forbid();
            }

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

            ItemToEdit = await _itemManager.GetItemAsync(id);
            if (ItemToEdit == null) return NotFound();

            var isAuthorized = await _authService.AuthorizeAsync(User, ItemToEdit, ItemOperations.Update);

            if (!isAuthorized.Succeeded)
            {
                _logger.LogWarning($"Użytkownik {User.Identity?.Name} próbował wykonać nieuprawnioną akcję {ItemOperations.Update.Name} na zasobie o ID {id}.");
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                await LoadSelects(string.Empty);
                return Page();
            }

            var parameters = await _staticDataManager.ConvertParametersAsync(
                Input.Title,
                Input.Grade,
                Input.Subject,
                Input.Level
            );

            var validatedImages = await Shared.ImageUploadValidation.ValidateAndReadAsync(
                Input.Images,
                requireAtLeastOne: false,
                ModelState);

            if (validatedImages == null)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                await LoadSelects(string.Empty);
                return Page();
            }

            ItemManager.Status result;
            try
            {
                // Keep reservation update in the same persistence operation as other edits.
                ItemToEdit.Reserved = Input.Reserved;

                result = await _itemManager.UpdateItemAsync(ItemToEdit, new ItemManager.ItemModel(
                    ItemToEdit.User,
                    parameters,
                    Input.Description ?? string.Empty,
                    Input.State,
                    Input.Price,
                    validatedImages.Streams,
                    validatedImages.Extensions,
                    ItemToEdit.Photo
                ));
            }
            catch (PhotoStorageException ex)
            {
                ModelState.AddModelError("Input.Images", ex.Message);
                Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await LoadSelects(string.Empty);
                return Page();
            }

            return ValidateAndReturn(ItemToEdit.Id, result);
        }

        public async Task<IActionResult> OnPostDeleteAsync(int itemId)
        {
            var itemToDelete = await _itemManager.GetItemAsync(itemId);
            if (itemToDelete == null) return NotFound();
            
            var isAuthorized = await _authService.AuthorizeAsync(User, itemToDelete, ItemOperations.Delete);
            if (!isAuthorized.Succeeded)
            {
                _logger.LogWarning($"Użytkownik {User.Identity?.Name} próbował wykonać nieuprawnioną akcję {ItemOperations.Delete.Name} na zasobie o ID {itemId}.");
                return Forbid();
            }

            await _itemManager.DeleteItemAsync(itemId);
            return RedirectToPage("/Browse");
        }
    }
}
