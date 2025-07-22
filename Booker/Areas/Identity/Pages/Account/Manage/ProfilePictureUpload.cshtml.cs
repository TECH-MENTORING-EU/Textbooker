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
using Microsoft.AspNetCore.Http; // Dodane dla IFormFile i StatusCodes

namespace Booker.Areas.Identity.Pages.Account.Manage
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
                return Redirect("/Identity/Account/Login");
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Changed: Added null/empty/parse checks for userIdString and userId
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Redirect("/Identity/Account/Login");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            // Changed: Use null-conditional operator or null coalescing if user.Photo can be null
            CurrentProfilePictureUrl = user.Photo ?? "https://cdn.pixabay.com/photo/2015/10/05/22/37/blank-profile-picture-973460_1280.png";

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var isUserAuthenticated = User.Identity?.IsAuthenticated ?? false;

            if (!isUserAuthenticated)
            {
                return Redirect("/Identity/Account/Login");
            }

            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Page();
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Changed: Added null/empty/parse checks for userIdString and userId
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                ModelState.AddModelError(string.Empty, "Nie uda³o siê zidentyfikowaæ u¿ytkownika. Proszê spróbowaæ ponownie.");
                await OnGetAsync();
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Page();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "U¿ytkownik nie znaleziony. Proszê spróbowaæ ponownie.");
                await OnGetAsync();
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Page();
            }

            if (Input.Image == null || Input.Image.Length == 0)
            {
                ModelState.AddModelError(nameof(Input.Image), "Proszê wybraæ zdjêcie do przes³ania.");
                await OnGetAsync();
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Page();
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(Input.Image.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError(nameof(Input.Image), "Dozwolone s¹ tylko pliki graficzne (jpg, jpeg, png, gif).");
                await OnGetAsync();
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Page();
            }

            const long maxFileSize = 5 * 1024 * 1024;
            if (Input.Image.Length > maxFileSize)
            {
                ModelState.AddModelError(nameof(Input.Image), "Plik jest zbyt du¿y (maks. 5MB).");
                await OnGetAsync();
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Page();
            }

            var containerName = _config["AzureStorage:ContainerName"];
            // Changed: Check for null/empty containerName
            if (string.IsNullOrEmpty(containerName))
            {
                ModelState.AddModelError(string.Empty, "Nazwa kontenera Azure Blob Storage nie jest skonfigurowana. Skontaktuj siê z administratorem.");
                await OnGetAsync();
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return Page();
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            try
            {
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                // Changed: Ensure user.Photo is not null before trying to delete old blob
                if (!string.IsNullOrEmpty(user.Photo))
                {
                    try
                    {
                        var oldBlobUri = new Uri(user.Photo);
                        var oldBlobName = Path.GetFileName(oldBlobUri.LocalPath);
                        var oldBlobClient = containerClient.GetBlobClient(oldBlobName);
                        await oldBlobClient.DeleteIfExistsAsync();
                    }
                    catch (Exception)
                    {
                        // Log the exception if needed, but don't fail the upload just because old file delete failed
                    }
                }

                var fileName = Guid.NewGuid().ToString() + fileExtension;
                var blobClient = containerClient.GetBlobClient(fileName);

                using (var stream = Input.Image.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }

                user.Photo = blobClient.Uri.ToString();
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Zdjêcie profilowe zosta³o pomyœlnie przes³ane!";
                return RedirectToPage();
            }
            catch (RequestFailedException rfe)
            {
                ModelState.AddModelError(string.Empty, $"Wyst¹pi³ b³¹d podczas operacji na Azure Storage: {rfe.Message}");
                await OnGetAsync();
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return Page();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Wyst¹pi³ b³¹d podczas zapisywania zdjêcia profilowego w bazie danych. Spróbuj ponownie.");
                await OnGetAsync();
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return Page();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Wyst¹pi³ nieoczekiwany b³¹d podczas przesy³ania zdjêcia. Spróbuj ponownie.");
                await OnGetAsync();
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return Page();
            }
        }
    }
}