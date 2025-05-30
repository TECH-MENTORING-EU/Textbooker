using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Booker.Pages
{
    public class UploadModel : PageModel
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _config;

        public UploadModel(BlobServiceClient blobServiceClient, IConfiguration config)
        {
            _blobServiceClient = blobServiceClient;
            _config = config;
        }

        public void OnGet()
        {
            // Just renders the page
        }

        public async Task<IActionResult> OnPost(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Content("No file uploaded.");

            var containerName = _config["AzureStorage:ContainerName"];
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(file.FileName);

            await using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true);

            if (Request.Headers["HX-Request"] == "true")
            {
                return Content($"✅ Uploaded successfully: <a href='{blobClient.Uri}' target='_blank'>{file.FileName}</a>", "text/html");
            }

            // Fallback non-htmx response
            return RedirectToPage();
        }
    }
}