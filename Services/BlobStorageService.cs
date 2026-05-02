using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace EventEase.Services
{
    // This service centralizes all the logic for saving and retrieving images from Blob Storage.
    public class BlobStorageService
    {
        // This is a connection helper object that will let us talk to Blob Storage.
        private readonly BlobServiceClient _blobServiceClient;

        // This holds the name of the container that is used for the images.
        private const string ContainerName = "venue-images";

        // Constructor: This runs when the service is first created for the application.
        // IOptions is a built-in .NET Core way to get the configuration from appsettings.json.
        public BlobStorageService(IConfiguration configuration)
        {
            // Get the 'BlobStorage' connection string from the appsettings.json file.
            var connectionString = configuration.GetConnectionString("BlobStorage");

            // Create the main client that connects to the local Azurite storage account.
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        // This helper method creates the container if it doesn't already exist.
        private async Task EnsureContainerExistsAsync()
        {
            // Get a client for the specific "venue-images" container.
            var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);

            // FIX: Pass PublicAccessType.Blob so every blob inside is publicly readable.
            // Without this, the container defaults to private access (None), meaning the browser
            // receives a 403 Forbidden error when it tries to load the image URL — even though
            // the blob was uploaded successfully. Setting Blob-level public access allows any
            // browser to GET a blob URL without needing an authentication token.
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
        }

        // This is the main method that will call from the VenueController.
        // It takes an uploaded file (IFormFile) and saves it to the container.
        public async Task<string> UploadImageAsync(IFormFile imageFile)
        {
            // 1. This makes sure the 'venue-images' container exists.
            await EnsureContainerExistsAsync();

            // Generate a unique name for the file to prevent any accidental overwriting.
            //    I use a GUID (globally unique identifier) and add the original file extension.
            var fileExtension = Path.GetExtension(imageFile.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

            // Get a reference to the blob (the file inside the container) I want to upload to.
            var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
            var blobClient = containerClient.GetBlobClient(uniqueFileName);

            // This line opens a stream to read the content of the uploaded file.
            //    The 'using' keyword ensures the file stream is properly closed its finished.
            using (var stream = imageFile.OpenReadStream())
            {
                // Upload the file's content stream to the local Blob Storage.
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = imageFile.ContentType });
            }

            // Return the URL of the uploaded blob. This URL will be saved in my database.
            return blobClient.Uri.ToString();
        }
    }
}