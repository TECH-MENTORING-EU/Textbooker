using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Booker.Data;
using Booker.Services;

namespace Booker.Areas.Identity.Pages.Account.Manage
{
    public class ProfilePictureUploadModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly PhotosManager _photosManager;
        private readonly ILogger<ProfilePictureUploadModel> _logger;

        public ProfilePictureUploadModel(
            UserManager<User> userManager,
            PhotosManager photosManager,
            ILogger<ProfilePictureUploadModel> logger)
        {
            _userManager = userManager;
            _photosManager = photosManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? CurrentProfilePictureUrl { get; set; }

        public class InputModel
        {
            [Display(Name = "Zdjęcie profilowe")]
            [Required(ErrorMessage = "Proszę wybrać plik do przesłania.")]
            [DataType(DataType.Upload)]
            public IFormFile? Image { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                var userId = _userManager.GetUserId(User);
                return NotFound($"Nie można załadować użytkownika o ID '{userId ?? "unknown"}'.");
            }

            CurrentProfilePictureUrl = user.Photo;
            return Page();
        }

        public async Task<IActionResult> OnPostUploadAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Attempted to upload profile picture for non-existent user");
                ModelState.AddModelError(string.Empty, "Brak autoryzacji.");
                return Page();
            }

            if (!ModelState.IsValid || Input.Image is null || Input.Image.Length == 0)
            {
                CurrentProfilePictureUrl = user.Photo;
                return Page();
            }

            try
            {
                if (!string.IsNullOrEmpty(user.Photo))
                {
                    try
                    {
                        await _photosManager.DeletePhotoAsync(user.Photo);
                        _logger.LogInformation("Successfully deleted old profile picture for user {UserId}", user.Id);
                    }
                    catch (Exception delEx)
                    {
                        _logger.LogWarning(delEx, "Failed to delete old profile picture: {PhotoUrl} for user {UserId}", user.Photo, user.Id);
                    }
                }

                using var stream = Input.Image.OpenReadStream();
                var newPhotoUri = await _photosManager.AddPhotoAsync(stream, ".jpeg");

                _logger.LogInformation("Successfully uploaded new profile picture for user {UserId}: {PhotoUrl}", user.Id, newPhotoUri);

                user.Photo = newPhotoUri.ToString();
                var updateResult = await _userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    _logger.LogError("Failed to update user profile with new photo URL for user {UserId}: {Errors}",
                        user.Id, string.Join(", ", updateResult.Errors.Select(e => e.Description)));

                    try
                    {
                        await _photosManager.DeletePhotoAsync(newPhotoUri.ToString());
                        _logger.LogInformation("Rolled back uploaded photo after user update failure");
                    }
                    catch (Exception rollbackEx)
                    {
                        _logger.LogError(rollbackEx, "Failed to rollback uploaded photo after user update failure");
                    }

                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    CurrentProfilePictureUrl = user.Photo;
                    return Page();
                }

                TempData["SuccessMessage"] = "Zdjęcie profilowe zostało zaktualizowane!";
                _logger.LogInformation("Profile picture successfully updated for user {UserId}", user.Id);

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating profile picture for user {UserId}", user?.Id.ToString() ?? "unknown");
                ModelState.AddModelError(string.Empty, "Wystąpił błąd podczas zapisywania zdjęcia. Spróbuj ponownie.");

                if (user != null)
                {
                    CurrentProfilePictureUrl = user.Photo;
                }

                return Page();
            }
        }
    }
}