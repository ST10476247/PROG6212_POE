using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Common;
using RaceDay.API.Data;
using RaceDay.API.Filters;
using RaceDay.API.Models.DTOs;
using RaceDay.API.Models.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [SwaggerTag("Participant Event Enrolments & Bib Allocation")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Participant enters a road event by selecting a race distance category
        /// </summary>
        /// <param name="dto">Category selection and emergency notes</param>
        [HttpPost]
        [RequireRole("Participant")]
        [SwaggerOperation(Summary = "Enrol into Event Category", Description = "Participant enrols into a category, receives automatic bib allocation and pending payment reference.")]
        [ProducesResponseType(typeof(EnrolmentResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> EnrolParticipant([FromBody] CreateEnrolmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userCtx = HttpContext.GetCurrentUser();
            var participant = await _context.Participants
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userCtx.UserId);

            if (participant == null)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Participant profile not found for current user." });
            }

            var category = await _context.EventCategories
                .Include(c => c.Event)
                .Include(c => c.Enrolments)
                .FirstOrDefaultAsync(c => c.CategoryId == dto.CategoryId);

            if (category == null)
                return NotFound(new { message = $"Category with ID {dto.CategoryId} was not found." });

            // Check if registration is open
            if (category.Event.Status == "Closed" || category.Event.Status == "Completed" || category.Event.Status == "Cancelled")
            {
                return Conflict(new { message = $"Registration is closed. Event status is '{category.Event.Status}'." });
            }

            if (category.Event.RegistrationClose.HasValue && DateTime.UtcNow > category.Event.RegistrationClose.Value)
            {
                return Conflict(new { message = "Registration closed for this event." });
            }

            // Check duplicate enrolment
            bool alreadyEnrolled = await _context.EventEnrolments
                .AnyAsync(en => en.ParticipantId == participant.ParticipantId && en.CategoryId == category.CategoryId && en.Status != "Withdrawn");

            if (alreadyEnrolled)
            {
                return Conflict(new { message = "You have already enrolled into this category." });
            }

            // Check capacity
            int currentEnrolled = category.Enrolments.Count(en => en.Status != "Withdrawn");
            if (currentEnrolled >= category.MaxCapacity)
            {
                return Conflict(new { message = "Category has reached its maximum entry capacity." });
            }

            // Generate Bib Number and Payment Reference
            string bibPrefix = category.CategoryName.Length > 0 ? category.CategoryName[0].ToString().ToUpper() : "R";
            int bibNum = currentEnrolled + 101;
            string bibNumber = $"{bibPrefix}-{bibNum:D4}";

            string paymentRef = $"PAY-{category.EventId}-{participant.ParticipantId}-{DateTime.UtcNow.Ticks % 1000000:D6}";

            var enrolment = new EventEnrolment
            {
                ParticipantId = participant.ParticipantId,
                EventId = category.EventId,
                CategoryId = category.CategoryId,
                EnrolmentDate = DateTime.UtcNow,
                BibNumber = bibNumber,
                PaymentStatus = "Pending",
                PaymentReference = paymentRef,
                Status = "Registered",
                EmergencyNotes = dto.EmergencyNotes ?? "None"
            };

            _context.EventEnrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            var response = new EnrolmentResponseDto
            {
                EnrolmentId = enrolment.EnrolmentId,
                ParticipantId = participant.ParticipantId,
                ParticipantName = participant.User.FullName,
                ParticipantEmail = participant.User.Email,
                EventId = category.EventId,
                EventTitle = category.Event.Title,
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                DistanceKm = category.DistanceKm,
                EnrolmentDate = enrolment.EnrolmentDate,
                BibNumber = enrolment.BibNumber,
                PaymentStatus = enrolment.PaymentStatus,
                PaymentReference = enrolment.PaymentReference,
                Status = enrolment.Status,
                EmergencyNotes = enrolment.EmergencyNotes
            };

            return CreatedAtAction(nameof(GetEnrolmentById), new { id = enrolment.EnrolmentId }, response);
        }

        /// <summary>
        /// Alias endpoint: Participant enrols into a category under specific event
        /// </summary>
        [HttpPost("/api/events/{eventId:int}/categories/{categoryId:int}/enrol")]
        [RequireRole("Participant")]
        [SwaggerOperation(Summary = "Enrol into Event Category (Hierarchical Route)", Description = "Hierarchical enrolment route matching the Part 1 endpoint plan.")]
        [ProducesResponseType(typeof(EnrolmentResponseDto), StatusCodes.Status201Created)]
        public Task<IActionResult> EnrolHierarchical(int eventId, int categoryId, [FromBody] CreateEnrolmentDto dto)
        {
            dto.CategoryId = categoryId;
            return EnrolParticipant(dto);
        }

        /// <summary>
        /// Retrieves all race entries for the logged-in participant
        /// </summary>
        [HttpGet("my")]
        [HttpGet("mine")]
        [RequireRole("Participant")]
        [SwaggerOperation(Summary = "Get My Enrolments", Description = "Participant retrieves all active and historical race entries and bib numbers.")]
        [ProducesResponseType(typeof(List<EnrolmentResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var userCtx = HttpContext.GetCurrentUser();
            var participant = await _context.Participants
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userCtx.UserId);

            if (participant == null)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Participant profile not found." });
            }

            var enrolments = await _context.EventEnrolments
                .Include(en => en.Event)
                .Include(en => en.Category)
                .Include(en => en.Result)
                .Where(en => en.ParticipantId == participant.ParticipantId)
                .OrderByDescending(en => en.EnrolmentDate)
                .Select(en => new EnrolmentResponseDto
                {
                    EnrolmentId = en.EnrolmentId,
                    ParticipantId = en.ParticipantId,
                    ParticipantName = participant.User.FullName,
                    ParticipantEmail = participant.User.Email,
                    EventId = en.EventId,
                    EventTitle = en.Event.Title,
                    CategoryId = en.CategoryId,
                    CategoryName = en.Category.CategoryName,
                    DistanceKm = en.Category.DistanceKm,
                    EnrolmentDate = en.EnrolmentDate,
                    BibNumber = en.BibNumber,
                    PaymentStatus = en.PaymentStatus,
                    PaymentReference = en.PaymentReference,
                    Status = en.Status,
                    EmergencyNotes = en.EmergencyNotes,
                    Result = en.Result != null ? new ResultDto
                    {
                        ResultId = en.Result.ResultId,
                        EnrolmentId = en.EnrolmentId,
                        ParticipantName = participant.User.FullName,
                        BibNumber = en.BibNumber,
                        EventTitle = en.Event.Title,
                        CategoryName = en.Category.CategoryName,
                        FinishTimeSec = en.Result.FinishTimeSec,
                        ChipTimeSec = en.Result.ChipTimeSec,
                        FinishTimeFormatted = ResultDto.FormatSeconds(en.Result.FinishTimeSec),
                        ChipTimeFormatted = ResultDto.FormatSeconds(en.Result.ChipTimeSec),
                        GunPosition = en.Result.GunPosition,
                        CategoryPosition = en.Result.CategoryPosition,
                        GenderPosition = en.Result.GenderPosition,
                        Status = en.Result.Status,
                        Splits = en.Result.Splits,
                        RecordedAt = en.Result.RecordedAt
                    } : null
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        /// <summary>
        /// Retrieves an individual enrolment entry ticket
        /// </summary>
        [HttpGet("{id:int}")]
        [RequireAuth]
        [SwaggerOperation(Summary = "Get Enrolment By ID", Description = "Participant retrieves own ticket, or Organiser views entrant on their event.")]
        [ProducesResponseType(typeof(EnrolmentResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEnrolmentById(int id)
        {
            var en = await _context.EventEnrolments
                .Include(en => en.Participant)
                    .ThenInclude(p => p.User)
                .Include(en => en.Event)
                    .ThenInclude(e => e.Organiser)
                .Include(en => en.Category)
                .Include(en => en.Result)
                .FirstOrDefaultAsync(e => e.EnrolmentId == id);

            if (en == null)
                return NotFound(new { message = $"Enrolment with ID {id} not found." });

            var userCtx = HttpContext.GetCurrentUser();

            // Authorization check: participant sees own; organiser sees if they own event
            if (userCtx.Role == "Participant")
            {
                if (en.Participant.UserId != userCtx.UserId)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "Forbidden. You cannot view another athlete's entry." });
                }
            }
            else if (userCtx.Role == "Organiser")
            {
                if (en.Event.Organiser.UserId != userCtx.UserId)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "Forbidden. You are not the organiser of this event." });
                }
            }

            var response = new EnrolmentResponseDto
            {
                EnrolmentId = en.EnrolmentId,
                ParticipantId = en.ParticipantId,
                ParticipantName = en.Participant.User.FullName,
                ParticipantEmail = en.Participant.User.Email,
                EventId = en.EventId,
                EventTitle = en.Event.Title,
                CategoryId = en.CategoryId,
                CategoryName = en.Category.CategoryName,
                DistanceKm = en.Category.DistanceKm,
                EnrolmentDate = en.EnrolmentDate,
                BibNumber = en.BibNumber,
                PaymentStatus = en.PaymentStatus,
                PaymentReference = en.PaymentReference,
                Status = en.Status,
                EmergencyNotes = en.EmergencyNotes
            };

            return Ok(response);
        }

        /// <summary>
        /// Updates an enrolment status, bib number, or payment status
        /// </summary>
        [HttpPut("{id:int}")]
        [HttpPut("{id:int}/status")]
        [RequireAuth]
        [SwaggerOperation(Summary = "Update Enrolment Status", Description = "Organisers adjust bib or payment status; Participants can mark status as Withdrawn.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateEnrolmentStatus(int id, [FromBody] UpdateEnrolmentStatusDto dto)
        {
            var en = await _context.EventEnrolments
                .Include(en => en.Participant)
                .Include(en => en.Event)
                    .ThenInclude(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.EnrolmentId == id);

            if (en == null)
                return NotFound(new { message = $"Enrolment with ID {id} not found." });

            var userCtx = HttpContext.GetCurrentUser();

            if (userCtx.Role == "Participant")
            {
                if (en.Participant.UserId != userCtx.UserId)
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "Forbidden. Not your enrolment." });

                // Participants may only withdraw
                if (!string.IsNullOrWhiteSpace(dto.Status) && dto.Status.Equals("Withdrawn", StringComparison.OrdinalIgnoreCase))
                {
                    if (DateTime.UtcNow > en.Event.StartDate.AddDays(-7))
                    {
                        return BadRequest(new { message = "Withdrawals are only permitted up to 7 days before event start." });
                    }
                    en.Status = "Withdrawn";
                }
                else
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "Participants can only request a withdrawal status change." });
                }
            }
            else if (userCtx.Role == "Organiser")
            {
                if (en.Event.Organiser.UserId != userCtx.UserId)
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "Forbidden. Not your event." });

                if (!string.IsNullOrWhiteSpace(dto.BibNumber)) en.BibNumber = dto.BibNumber.Trim();
                if (!string.IsNullOrWhiteSpace(dto.PaymentStatus)) en.PaymentStatus = dto.PaymentStatus.Trim();
                if (!string.IsNullOrWhiteSpace(dto.Status)) en.Status = dto.Status.Trim();
                if (dto.EmergencyNotes != null) en.EmergencyNotes = dto.EmergencyNotes;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Enrolment updated successfully.", enrolmentId = en.EnrolmentId });
        }

        /// <summary>
        /// Removes an enrolment (Organiser only; allowed only while payment is Pending)
        /// </summary>
        [HttpDelete("{id:int}")]
        [RequireRole("Organiser")]
        [SwaggerOperation(Summary = "Delete Enrolment", Description = "Removes a pending enrolment record. Refused if payment is Paid.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteEnrolment(int id)
        {
            var en = await _context.EventEnrolments
                .Include(e => e.Event)
                    .ThenInclude(ev => ev.Organiser)
                .FirstOrDefaultAsync(e => e.EnrolmentId == id);

            if (en == null)
                return NotFound(new { message = $"Enrolment with ID {id} not found." });

            var userCtx = HttpContext.GetCurrentUser();
            if (en.Event.Organiser.UserId != userCtx.UserId)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Forbidden. You are not the organiser of this event." });

            if (en.PaymentStatus == "Paid")
                return Conflict(new { message = "Cannot hard-delete a paid enrolment. Update status to Cancelled or Refunded instead." });

            _context.EventEnrolments.Remove(en);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
