using System.IO;
using System.Threading.Tasks;

namespace RaceDay.API.Services
{
    /// <summary>
    /// Service contract for Azure Blob Storage operations in RaceDay.API
    /// </summary>
    public interface IBlobStorageService
    {
        /// <summary>
        /// Uploads an event banner image and returns the public blob URL
        /// </summary>
        Task<string> UploadBannerImageAsync(Stream stream, string originalFileName, string contentType);

        /// <summary>
        /// Uploads a user or participant profile picture and returns the public blob URL
        /// </summary>
        Task<string> UploadProfilePictureAsync(Stream stream, string originalFileName, string contentType);

        /// <summary>
        /// Deletes an image from blob storage
        /// </summary>
        Task<bool> DeleteImageAsync(string fileUrl, string containerName);
    }
}

