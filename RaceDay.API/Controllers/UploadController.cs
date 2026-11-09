using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Filters;
using RaceDay.API.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [RequireAuth]
    [SwaggerTag("Azure Blob Storage Cloud File Uploads")]
    public class UploadController : ControllerBase
    {
        private readonly IBlobStorageService _blobService;

        public UploadController(IBlobStorageService blobService)
        {
            _blobService = blobService;
        }

        /// <summary>
        /// Upload an event banner image to Azure Blob Storage
        /// </summary>
        /// <param name="file">Image file (JPEG, PNG, WEBP)</param>
        [HttpPost("banner")]
        [RequireRole("Organiser")]
        [SwaggerOperation(Summary = "Upload Banner Image", Description = "Uploads event poster/banner image to Azure Blob Storage container 'raceday-banners' and returns public URL.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadBanner([FromForm] IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No image file provided." });

            if (file.Length > 10 * 1024 * 1024)
                return BadRequest(new { message = "Image file size exceeds the 10MB limit." });

            string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".webp" && ext != ".gif")
                return BadRequest(new { message = "Unsupported image format. Allowed formats: .jpg, .jpeg, .png, .webp, .gif" });

            using var stream = file.OpenReadStream();
            string url = await _blobService.UploadBannerImageAsync(stream, file.FileName, file.ContentType);

            return Ok(new { url, message = "Banner uploaded successfully to Azure Blob Storage." });
        }

        /// <summary>
        /// Upload a participant or user profile picture to Azure Blob Storage
        /// </summary>
        /// <param name="file">Image file (JPEG, PNG, WEBP)</param>
        [HttpPost("avatar")]
        [SwaggerOperation(Summary = "Upload Profile Picture", Description = "Uploads user profile picture to Azure Blob Storage container 'raceday-profiles' and returns public URL.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadAvatar([FromForm] IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No image file provided." });

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { message = "Avatar file size exceeds the 5MB limit." });

            string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".webp")
                return BadRequest(new { message = "Unsupported image format. Allowed formats: .jpg, .jpeg, .png, .webp" });

            using var stream = file.OpenReadStream();
            string url = await _blobService.UploadProfilePictureAsync(stream, file.FileName, file.ContentType);

            return Ok(new { url, message = "Profile picture uploaded successfully to Azure Blob Storage." });
        }
    }
}

