using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Common;
using RaceDay.API.Data;
using RaceDay.API.Models.DTOs;
using RaceDay.API.Models.Entities;
using RaceDay.API.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [SwaggerTag("Authentication & User Session Management")]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;

        public AuthController(RaceDayDbContext context, IPasswordHasher passwordHasher, ITokenService tokenService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Register a new athlete / participant account
        /// </summary>
        /// <param name="dto">Participant registration details</param>
        /// <response code="201">Participant account successfully created and session established</response>
        /// <response code="400">Validation failure</response>
        /// <response code="409">Email or South African ID already registered</response>
        [HttpPost("register/participant")]
        [SwaggerOperation(Summary = "Register Participant", Description = "Registers a new runner, walker, or cyclist profile and initializes an active session.")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> RegisterParticipant([FromBody] RegisterParticipantDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string normalizedEmail = dto.Email.Trim().ToLowerInvariant();

            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail))
                return Conflict(new { message = $"Email '{dto.Email}' is already registered." });

            if (!string.IsNullOrEmpty(dto.SAIDOrPassport) &&
                await _context.Participants.AnyAsync(p => p.SAIDOrPassport == dto.SAIDOrPassport))
            {
                return Conflict(new { message = $"ID/Passport '{dto.SAIDOrPassport}' is already linked to another participant." });
            }

            var user = new User
            {
                Email = normalizedEmail,
                PasswordHash = _passwordHasher.HashPassword(dto.Password),
                Role = "Participant",
                FullName = dto.FullName.Trim(),
                PhoneNumber = dto.PhoneNumber,
                Province = dto.Province,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var participant = new Participant
            {
                UserId = user.UserId,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender ?? "Other",
                SAIDOrPassport = dto.SAIDOrPassport,
                ClubName = dto.ClubName,
                EmergencyContactName = dto.EmergencyContactName,
                EmergencyContactNo = dto.EmergencyContactNo,
                TshirtSize = dto.TshirtSize
            };

            _context.Participants.Add(participant);
            await _context.SaveChangesAsync();

            // Establish Server-Side Session
            HttpContext.Session?.SetUserSession(user.UserId, user.Email, user.FullName, user.Role, null, participant.ParticipantId);

            string token = _tokenService.GenerateToken(user, null, participant.ParticipantId, out DateTime expiresAt);

            var response = new AuthResponseDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                Token = token,
                ExpiresAt = expiresAt,
                ParticipantId = participant.ParticipantId
            };

            return CreatedAtAction(nameof(RegisterParticipant), new { id = user.UserId }, response);
        }

        /// <summary>
        /// Register a new event organiser / club director account
        /// </summary>
        /// <param name="dto">Organiser company and contact details</param>
        /// <response code="201">Organiser account created and session established</response>
        /// <response code="400">Validation failure</response>
        /// <response code="409">Email or organisation already exists</response>
        [HttpPost("register/organiser")]
        [SwaggerOperation(Summary = "Register Organiser", Description = "Registers a new event organiser profile (e.g. Comrades or club director) with verified status.")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> RegisterOrganiser([FromBody] RegisterOrganiserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string normalizedEmail = dto.Email.Trim().ToLowerInvariant();

            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail))
                return Conflict(new { message = $"Email '{dto.Email}' is already registered." });

            var user = new User
            {
                Email = normalizedEmail,
                PasswordHash = _passwordHasher.HashPassword(dto.Password),
                Role = "Organiser",
                FullName = dto.FullName.Trim(),
                PhoneNumber = dto.PhoneNumber,
                Province = dto.Province,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var organiser = new Organiser
            {
                UserId = user.UserId,
                CompanyName = dto.CompanyName.Trim(),
                CompanyRegNo = dto.CompanyRegNo,
                ContactEmail = string.IsNullOrWhiteSpace(dto.ContactEmail) ? user.Email : dto.ContactEmail.Trim(),
                IsVerified = true
            };

            _context.Organisers.Add(organiser);
            await _context.SaveChangesAsync();

            // Establish Server-Side Session
            HttpContext.Session?.SetUserSession(user.UserId, user.Email, user.FullName, user.Role, organiser.OrganiserId, null);

            string token = _tokenService.GenerateToken(user, organiser.OrganiserId, null, out DateTime expiresAt);

            var response = new AuthResponseDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                Token = token,
                ExpiresAt = expiresAt,
                OrganiserId = organiser.OrganiserId
            };

            return CreatedAtAction(nameof(RegisterOrganiser), new { id = user.UserId }, response);
        }

        /// <summary>
        /// Authenticate user credentials and start an authenticated session
        /// </summary>
        /// <param name="dto">Login credentials</param>
        /// <response code="200">Login successful, returns JWT bearer token and user role</response>
        /// <response code="401">Invalid email or password</response>
        [HttpPost("login")]
        [SwaggerOperation(Summary = "User Login", Description = "Authenticates credentials, starts server-side session, and issues a JWT token with Role claims.")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string normalizedEmail = dto.Email.Trim().ToLowerInvariant();

            var user = await _context.Users
                .Include(u => u.OrganiserProfile)
                .Include(u => u.ParticipantProfile)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

            if (user == null || !_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid email or password. Please check your credentials." });
            }

            if (!user.IsActive)
            {
                return Unauthorized(new { message = "This account is inactive. Please contact support." });
            }

            int? organiserId = user.OrganiserProfile?.OrganiserId;
            int? participantId = user.ParticipantProfile?.ParticipantId;

            // Maintain user authenticated state and role in server session
            HttpContext.Session?.SetUserSession(user.UserId, user.Email, user.FullName, user.Role, organiserId, participantId);

            string token = _tokenService.GenerateToken(user, organiserId, participantId, out DateTime expiresAt);

            var response = new AuthResponseDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                Token = token,
                ExpiresAt = expiresAt,
                OrganiserId = organiserId,
                ParticipantId = participantId
            };

            return Ok(response);
        }

        /// <summary>
        /// Terminate active session and log out
        /// </summary>
        [HttpPost("logout")]
        [SwaggerOperation(Summary = "User Logout", Description = "Clears the active session state from the server.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Logout()
        {
            HttpContext.Session?.ClearUserSession();
            return Ok(new { message = "Logged out successfully. Session state cleared." });
        }

        /// <summary>
        /// Inspect current server-side session status
        /// </summary>
        [HttpGet("session")]
        [SwaggerOperation(Summary = "Session Status", Description = "Returns current authenticated session user and active role.")]
        [ProducesResponseType(typeof(CurrentUserContext), StatusCodes.Status200OK)]
        public IActionResult GetSession()
        {
            var user = HttpContext.GetCurrentUser();
            return Ok(user);
        }

        /// <summary>
        /// Refreshes JWT authentication token
        /// </summary>
        [HttpPost("refresh")]
        [SwaggerOperation(Summary = "Refresh Token", Description = "Refreshes an active authentication session.")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            var userCtx = HttpContext.GetCurrentUser();
            if (!userCtx.IsAuthenticated)
                return Unauthorized(new { message = "Session expired or invalid." });

            var user = await _context.Users
                .Include(u => u.OrganiserProfile)
                .Include(u => u.ParticipantProfile)
                .FirstOrDefaultAsync(u => u.UserId == userCtx.UserId);

            if (user == null)
                return Unauthorized(new { message = "User record not found." });

            int? organiserId = user.OrganiserProfile?.OrganiserId;
            int? participantId = user.ParticipantProfile?.ParticipantId;

            string token = _tokenService.GenerateToken(user, organiserId, participantId, out DateTime expiresAt);

            return Ok(new AuthResponseDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                Token = token,
                ExpiresAt = expiresAt,
                OrganiserId = organiserId,
                ParticipantId = participantId
            });
        }
    }
}
