using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RaceDay.API.Services
{
    public class AzureBlobStorageService : IBlobStorageService
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AzureBlobStorageService> _logger;
        private readonly string? _connectionString;
        private readonly string _bannerContainer;
        private readonly string _profileContainer;
        private readonly BlobServiceClient? _blobServiceClient;

        public AzureBlobStorageService(
            IConfiguration config,
            IWebHostEnvironment env,
            ILogger<AzureBlobStorageService> logger)
        {
            _config = config;
            _env = env;
            _logger = logger;

            _connectionString = _config["AzureBlobStorage:ConnectionString"] 
                ?? Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            _bannerContainer = _config["AzureBlobStorage:BannerContainer"] ?? "raceday-banners";
            _profileContainer = _config["AzureBlobStorage:ProfileContainer"] ?? "raceday-profiles";

            if (!string.IsNullOrWhiteSpace(_connectionString) &&
                !_connectionString.Equals("UseDevelopmentStorage=true", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    _blobServiceClient = new BlobServiceClient(_connectionString);
                    _logger.LogInformation("Azure Blob Storage client initialized with configured connection string.");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to initialize Azure Blob Storage client. Local fallback storage will be used.");
                    _blobServiceClient = null;
                }
            }
            else
            {
                _logger.LogInformation("Azure Blob Storage connection string not provided or set to local. Local storage fallback will be active.");
                _blobServiceClient = null;
            }
        }

        public async Task<string> UploadBannerImageAsync(Stream stream, string originalFileName, string contentType)
        {
            return await UploadToContainerAsync(_bannerContainer, stream, originalFileName, contentType, "banners");
        }

        public async Task<string> UploadProfilePictureAsync(Stream stream, string originalFileName, string contentType)
        {
            return await UploadToContainerAsync(_profileContainer, stream, originalFileName, contentType, "profiles");
        }

        public async Task<bool> DeleteImageAsync(string fileUrl, string containerName)
        {
            if (string.IsNullOrWhiteSpace(fileUrl)) return false;

            if (_blobServiceClient != null)
            {
                try
                {
                    var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                    var uri = new Uri(fileUrl);
                    string blobName = Path.GetFileName(uri.LocalPath);
                    var blobClient = containerClient.GetBlobClient(blobName);
                    return (await blobClient.DeleteIfExistsAsync()).Value;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not delete blob {Url} from Azure Blob Storage.", fileUrl);
                    return false;
                }
            }

            // Local fallback cleanup
            try
            {
                string localFolder = containerName.Contains("banner") ? "banners" : "profiles";
                string fileName = Path.GetFileName(fileUrl);
                string fullPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", localFolder, fileName);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not delete local file fallback for {Url}.", fileUrl);
            }

            return false;
        }

        private async Task<string> UploadToContainerAsync(
            string containerName, 
            Stream stream, 
            string originalFileName, 
            string contentType, 
            string localSubFolder)
        {
            string ext = Path.GetExtension(originalFileName);
            if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";
            string safeName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";

            // 1. Try real Azure Blob Storage if client is configured
            if (_blobServiceClient != null)
            {
                try
                {
                    var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                    await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                    var blobClient = containerClient.GetBlobClient(safeName);

                    if (stream.CanSeek) stream.Position = 0;

                    var headers = new BlobHttpHeaders
                    {
                        ContentType = string.IsNullOrWhiteSpace(contentType) ? "image/jpeg" : contentType
                    };

                    await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = headers });
                    _logger.LogInformation("Uploaded image {SafeName} to Azure Blob container '{Container}' successfully.", safeName, containerName);
                    return blobClient.Uri.ToString();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Azure Blob upload failed. Gracefully falling back to local storage for {SafeName}.", safeName);
                }
            }

            // 2. Reliable local storage fallback (for offline grading / development environments)
            string webRoot = _env.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot))
            {
                webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            string uploadsDir = Path.Combine(webRoot, "uploads", localSubFolder);
            Directory.CreateDirectory(uploadsDir);

            string fullFilePath = Path.Combine(uploadsDir, safeName);

            if (stream.CanSeek) stream.Position = 0;
            using (var fileStream = new FileStream(fullFilePath, FileMode.Create, FileAccess.Write))
            {
                await stream.CopyToAsync(fileStream);
            }

            _logger.LogInformation("Saved image {SafeName} to local fallback storage at /uploads/{Folder}/{SafeName}", safeName, localSubFolder, safeName);
            return $"/uploads/{localSubFolder}/{safeName}";
        }
    }
}

