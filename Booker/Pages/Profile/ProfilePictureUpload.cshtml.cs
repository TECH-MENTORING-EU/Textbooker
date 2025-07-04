using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Booker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.IO;
using Azure;

namespace Booker.Pages.Profile
{
    public class ProfilePictureUploadModel : PageModel
    {
        private readonly DataContext _context;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _config;
        private readonly ILogger<ProfilePictureUploadModel> _logger;

        public string? CurrentProfilePictureUrl { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = null!;

        public class InputModel
        {
            [Required(ErrorMessage = "Proszê przes³aæ zdjêcie profilowe.")]
            [DataType(DataType.Upload)]
            [Display(Name = "Zdjêcie profilowe")]
            public required IFormFile Image { get; set; } = null!;
        }

        public ProfilePictureUploadModel(DataContext context, BlobServiceClient blobServiceClient, IConfiguration config, ILogger<ProfilePictureUploadModel> logger)
        {
            _context = context;
            _blobServiceClient = blobServiceClient;
            _config = config;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var isUserAuthenticated = User.Identity?.IsAuthenticated ?? false;

            if (!isUserAuthenticated)
            {
                _logger.LogWarning("U¿ytkownik niezalogowany próbuje uzyskaæ dostêp do strony uploadu zdjêcia profilowego.");
                return Redirect("/Identity/Account/Login");
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                _logger.LogError("Nie mo¿na pobraæ ID u¿ytkownika z tokenu uwierzytelniaj¹cego.");
                return Redirect("/Identity/Account/Login");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                _logger.LogError("U¿ytkownik o ID {UserId} nie zosta³ znaleziony w bazie danych.", userId);
                return NotFound();
            }

            CurrentProfilePictureUrl = user.Photo;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var isUserAuthenticated = User.Identity?.IsAuthenticated ?? false;

            if (!isUserAuthenticated)
            {
                _logger.LogWarning("U¿ytkownik niezalogowany próbuje przes³aæ zdjêcie profilowe.");
                return Redirect("/Identity/Account/Login");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("B³êdy walidacji formularza podczas przesy³ania zdjêcia profilowego.");
                await OnGetAsync();
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Page();
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                ModelState.AddModelError(string.Empty, "Nie uda³o siê zidentyfikowaæ u¿ytkownika. Proszê spróbowaæ ponownie.");
                _logger.LogError("Nie mo¿na pobraæ ID u¿ytkownika z tokenu uwierzytelniaj¹cego podczas POST.");
                await OnGetAsync();
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Page();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "U¿ytkownik nie znaleziony. Proszê spróbowaæ ponownie.");
                _logger.LogError("U¿ytkownik o ID {UserId} nie zosta³ znaleziony w bazie danych podczas POST.", userId);
                await OnGetAsync();
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Page();
            }

            if (Input.Image == null || Input.Image.Length == 0)
            {
                ModelState.AddModelError(nameof(Input.Image), "Proszê wybraæ zdjêcie do przes³ania.");
                _logger.LogWarning("Brak pliku obrazu w ¿¹daniu POST po walidacji modelu.");
                await OnGetAsync();
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Page();
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(Input.Image.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError(nameof(Input.Image), "Dozwolone s¹ tylko pliki graficzne (jpg, jpeg, png, gif).");
                _logger.LogWarning("Próba przes³ania niedozwolonego typu pliku (serwer): {FileExtension}", fileExtension);
                await OnGetAsync();
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Page();
            }

            const long maxFileSize = 5 * 1024 * 1024;
            if (Input.Image.Length > maxFileSize)
            {
                ModelState.AddModelError(nameof(Input.Image), "Plik jest zbyt du¿y (maks. 5MB).");
                _logger.LogWarning("Próba przes³ania pliku wiêkszego ni¿ limit (serwer): {FileSize} bytes", Input.Image.Length);
                await OnGetAsync();
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Page();
            }

            var containerName = _config["AzureStorage:ContainerName"];
            if (string.IsNullOrEmpty(containerName))
            {
                ModelState.AddModelError(string.Empty, "Nazwa kontenera Azure Blob Storage nie jest skonfigurowana. Skontaktuj siê z administratorem.");
                _logger.LogError("Nazwa kontenera Azure Blob Storage ('AzureStorage:ContainerName') nie jest skonfigurowana w appsettings.");
                await OnGetAsync();
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return Page();
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            try
            {
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
                _logger.LogInformation("Sprawdzono/utworzono kontener Azure Blob Storage: {ContainerName}", containerName);

                if (!string.IsNullOrEmpty(user.Photo))
                {
                    try
                    {
                        var oldBlobUri = new Uri(user.Photo);
                        var oldBlobName = Path.GetFileName(oldBlobUri.LocalPath);
                        var oldBlobClient = containerClient.GetBlobClient(oldBlobName);
                        bool deleted = await oldBlobClient.DeleteIfExistsAsync();
                        if (deleted)
                        {
                            _logger.LogInformation("Usuniêto stare zdjêcie profilowe u¿ytkownika {UserId}: {OldBlobName}", userId, oldBlobName);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "B³¹d podczas usuwania starego zdjêcia profilowego u¿ytkownika {UserId}.", userId);
                    }
                }

                var fileName = Guid.NewGuid().ToString() + fileExtension;
                var blobClient = containerClient.GetBlobClient(fileName);

                using (var stream = Input.Image.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }

                _logger.LogInformation("Nowe zdjêcie profilowe u¿ytkownika {UserId} przes³ane do Azure Blob Storage: {BlobUri}", userId, blobClient.Uri.ToString());

                user.Photo = blobClient.Uri.ToString();
                await _context.SaveChangesAsync();
                _logger.LogInformation("Zaktualizowano URL zdjêcia profilowego w bazie danych dla u¿ytkownika {UserId}.", userId);

                TempData["SuccessMessage"] = "Zdjêcie profilowe zosta³o pomyœlnie przes³ane!";
                return RedirectToPage("/Profile/ProfilePictureUpload");
            }
            catch (RequestFailedException rfe)
            {
                ModelState.AddModelError(string.Empty, $"Wyst¹pi³ b³¹d podczas operacji na Azure Storage: {rfe.Message}");
                _logger.LogError(rfe, "B³¹d RequestFailedException podczas operacji na Azure Blob Storage dla u¿ytkownika {UserId}.", userId);
                await OnGetAsync();
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return Page();
            }
            catch (DbUpdateException dbEx)
            {
                ModelState.AddModelError(string.Empty, "Wyst¹pi³ b³¹d podczas zapisywania zdjêcia profilowego w bazie danych. Spróbuj ponownie.");
                _logger.LogError(dbEx, "B³¹d DbUpdateException podczas zapisywania URL zdjêcia dla u¿ytkownika {UserId}.", userId);
                await OnGetAsync();
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Wyst¹pi³ nieoczekiwany b³¹d podczas przesy³ania zdjêcia. Spróbuj ponownie.");
                _logger.LogError(ex, "Nieoczekiwany b³¹d podczas przesy³ania zdjêcia profilowego dla u¿ytkownika {UserId}.", userId);
                await OnGetAsync();
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return Page();
            }
        }
    }
}
