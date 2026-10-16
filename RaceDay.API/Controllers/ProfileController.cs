using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Common;
using RaceDay.API.Data;
using RaceDay.API.Filters;
using RaceDay.API.Models.DTOs;
using RaceDay.API.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [RequireAuth]
    [SwaggerTag("User Profile & Account Settings")]
    public class ProfileController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public ProfileController(RaceDayDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        /// <summary>
        /// Retrieves the authenticated user profile with role-specific details
        /// </summary>
        /// <response code="200">User profile returned</response>
        /// <response code="401">Unauthorized - user session not found</response>
        [HttpGet("me")]
        [SwaggerOperation(Summary = "Get Current Profile", Description = "Returns profile details for the logged-in user, including role-specific Organiser or Participant data.")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetProfile()
        {
            var userCtx = HttpContext.GetCurrentUser();

            var user = await _context.Users
                .Include(u => u.OrganiserProfile)
                .Include(u => u.ParticipantProfile)
                .FirstOrDefaultAsync(u => u.UserId == userCtx.UserId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            var dto = new UserProfileDto
            {
                UserId = user.UserId,
                Email = user.Email,
                Role = user.Role,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Province = user.Province,
                CreatedAt = user.CreatedAt
            };

            if (user.OrganiserProfile != null)
            {
                dto.OrganiserProfile = new OrganiserDetailDto
                {
                    OrganiserId = user.OrganiserProfile.OrganiserId,
                    CompanyName = user.OrganiserProfile.CompanyName,
                    CompanyRegNo = user.OrganiserProfile.CompanyRegNo,
                    ContactEmail = user.OrganiserProfile.ContactEmail,
                    IsVerified = user.OrganiserProfile.IsVerified
                };
            }

            if (user.ParticipantProfile != null)
            {
                dto.ParticipantProfile = new ParticipantDetailDto
                {
                    ParticipantId = user.ParticipantProfile.ParticipantId,
                    DateOfBirth = user.ParticipantProfile.DateOfBirth,
                    Gender = user.ParticipantProfile.Gender,
                    SAIDOrPassport = user.ParticipantProfile.SAIDOrPassport,
                    ClubName = user.ParticipantProfile.ClubName,
                    EmergencyContactName = user.ParticipantProfile.EmergencyContactName,
                    EmergencyContactNo = user.ParticipantProfile.EmergencyContactNo,
                    TshirtSize = user.ParticipantProfile.TshirtSize
                };
            }

            return Ok(dto);
        }

        /// <summary>
        /// Updates the authenticated user profile details
        /// </summary>
        /// <param name="dto">Fields to update</param>
        /// <response code="200">Profile successfully updated</response>
        /// <response code="400">Validation error</response>
        [HttpPut("me")]
        [SwaggerOperation(Summary = "Update Profile", Description = "Updates user contact details and role-specific information.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userCtx = HttpContext.GetCurrentUser();

            var user = await _context.Users
                .Include(u => u.OrganiserProfile)
                .Include(u => u.ParticipantProfile)
                .FirstOrDefaultAsync(u => u.UserId == userCtx.UserId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            if (!string.IsNullOrWhiteSpace(dto.FullName)) user.FullName = dto.FullName.Trim();
            if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber;
            if (dto.Province != null) user.Province = dto.Province;

            if (user.OrganiserProfile != null)
            {
                if (!string.IsNullOrWhiteSpace(dto.CompanyName)) user.OrganiserProfile.CompanyName = dto.CompanyName.Trim();
                if (dto.CompanyRegNo != null) user.OrganiserProfile.CompanyRegNo = dto.CompanyRegNo;
                if (!string.IsNullOrWhiteSpace(dto.ContactEmail)) user.OrganiserProfile.ContactEmail = dto.ContactEmail.Trim();
            }

            if (user.ParticipantProfile != null)
            {
                if (dto.ClubName != null) user.ParticipantProfile.ClubName = dto.ClubName;
                if (dto.EmergencyContactName != null) user.ParticipantProfile.EmergencyContactName = dto.EmergencyContactName;
                if (dto.EmergencyContactNo != null) user.ParticipantProfile.EmergencyContactNo = dto.EmergencyContactNo;
                if (dto.TshirtSize != null) user.ParticipantProfile.TshirtSize = dto.TshirtSize;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Profile updated successfully." });
        }

        /// <summary>
        /// Rotates user security password
        /// </summary>
        /// <param name="dto">Current and new password</param>
        /// <response code="200">Password successfully updated</response>
        /// <response code="400">New password does not meet complexity requirements</response>
        /// <response code="401">Current password does not match</response>
        [HttpPut("change-password")]
        [HttpPost("change-password")]
        [SwaggerOperation(Summary = "Change Password", Description = "Updates password after verifying the existing password.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userCtx = HttpContext.GetCurrentUser();
            var user = await _context.Users.FindAsync(userCtx.UserId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            if (!_passwordHasher.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
            {
                return Unauthorized(new { message = "Current password verification failed. Please try again." });
            }

            user.PasswordHash = _passwordHasher.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password changed successfully." });
        }
    }
}
