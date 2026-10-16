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
    [SwaggerTag("Race Day Results & Official Leaderboards")]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves an individual timing result certificate by result ID
        /// </summary>
        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Get Result By ID", Description = "Returns gun and chip finish times, category rankings, and split checkpoints.")]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetResultById(int id)
        {
            var r = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(en => en.Participant)
                        .ThenInclude(p => p.User)
                .Include(r => r.Enrolment)
                    .ThenInclude(en => en.Event)
                .Include(r => r.Enrolment)
                    .ThenInclude(en => en.Category)
                .FirstOrDefaultAsync(res => res.ResultId == id);

            if (r == null)
                return NotFound(new { message = $"Result with ID {id} not found." });

            var dto = new ResultDto
            {
                ResultId = r.ResultId,
                EnrolmentId = r.EnrolmentId,
                ParticipantName = r.Enrolment.Participant.User.FullName,
                BibNumber = r.Enrolment.BibNumber,
                EventTitle = r.Enrolment.Event.Title,
                CategoryName = r.Enrolment.Category.CategoryName,
                FinishTimeSec = r.FinishTimeSec,
                ChipTimeSec = r.ChipTimeSec,
                FinishTimeFormatted = ResultDto.FormatSeconds(r.FinishTimeSec),
                ChipTimeFormatted = ResultDto.FormatSeconds(r.ChipTimeSec),
                GunPosition = r.GunPosition,
                CategoryPosition = r.CategoryPosition,
                GenderPosition = r.GenderPosition,
                Status = r.Status,
                Splits = r.Splits,
                RecordedAt = r.RecordedAt
            };

            return Ok(dto);
        }

        /// <summary>
        /// Retrieves all personal race results for the logged-in athlete
        /// </summary>
        [HttpGet("my")]
        [RequireRole("Participant")]
        [SwaggerOperation(Summary = "Get My Results", Description = "Athlete retrieves historical finishes, chip times, and podium positions across all events.")]
        [ProducesResponseType(typeof(List<ResultDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyResults()
        {
            var userCtx = HttpContext.GetCurrentUser();
            var participant = await _context.Participants
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userCtx.UserId);

            if (participant == null)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Participant profile not found." });
            }

            var results = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(en => en.Event)
                .Include(r => r.Enrolment)
                    .ThenInclude(en => en.Category)
                .Where(r => r.Enrolment.ParticipantId == participant.ParticipantId)
                .OrderByDescending(r => r.Enrolment.Event.StartDate)
                .Select(r => new ResultDto
                {
                    ResultId = r.ResultId,
                    EnrolmentId = r.EnrolmentId,
                    ParticipantName = participant.User.FullName,
                    BibNumber = r.Enrolment.BibNumber,
                    EventTitle = r.Enrolment.Event.Title,
                    CategoryName = r.Enrolment.Category.CategoryName,
                    FinishTimeSec = r.FinishTimeSec,
                    ChipTimeSec = r.ChipTimeSec,
                    FinishTimeFormatted = ResultDto.FormatSeconds(r.FinishTimeSec),
                    ChipTimeFormatted = ResultDto.FormatSeconds(r.ChipTimeSec),
                    GunPosition = r.GunPosition,
                    CategoryPosition = r.CategoryPosition,
                    GenderPosition = r.GenderPosition,
                    Status = r.Status,
                    Splits = r.Splits,
                    RecordedAt = r.RecordedAt
                })
                .ToListAsync();

            return Ok(results);
        }

        /// <summary>
        /// Captures an official timing result for a registered entrant (Organiser only)
        /// </summary>
        [HttpPost]
        [RequireRole("Organiser")]
        [SwaggerOperation(Summary = "Capture Result", Description = "Organisers record finish times, gun position, and DNF/Finished status for a participant.")]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CaptureResult([FromBody] CreateResultDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var enrolment = await _context.EventEnrolments
                .Include(en => en.Event)
                    .ThenInclude(e => e.Organiser)
                .Include(en => en.Participant)
                    .ThenInclude(p => p.User)
                .Include(en => en.Category)
                .FirstOrDefaultAsync(en => en.EnrolmentId == dto.EnrolmentId);

            if (enrolment == null)
                return NotFound(new { message = $"Enrolment with ID {dto.EnrolmentId} not found." });

            var userCtx = HttpContext.GetCurrentUser();
            if (enrolment.Event.Organiser.UserId != userCtx.UserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Forbidden. You are not the organiser of this event." });
            }

            if (await _context.Results.AnyAsync(r => r.EnrolmentId == dto.EnrolmentId))
            {
                return Conflict(new { message = "A timing result has already been recorded for this enrolment. Use PUT to edit it." });
            }

            var result = new Result
            {
                EnrolmentId = dto.EnrolmentId,
                FinishTimeSec = dto.FinishTimeSec,
                ChipTimeSec = dto.ChipTimeSec,
                GunPosition = dto.GunPosition,
                CategoryPosition = dto.CategoryPosition,
                GenderPosition = dto.GenderPosition,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Finished" : dto.Status,
                Splits = dto.Splits,
                RecordedAt = DateTime.UtcNow
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            var responseDto = new ResultDto
            {
                ResultId = result.ResultId,
                EnrolmentId = result.EnrolmentId,
                ParticipantName = enrolment.Participant.User.FullName,
                BibNumber = enrolment.BibNumber,
                EventTitle = enrolment.Event.Title,
                CategoryName = enrolment.Category.CategoryName,
                FinishTimeSec = result.FinishTimeSec,
                ChipTimeSec = result.ChipTimeSec,
                FinishTimeFormatted = ResultDto.FormatSeconds(result.FinishTimeSec),
                ChipTimeFormatted = ResultDto.FormatSeconds(result.ChipTimeSec),
                GunPosition = result.GunPosition,
                CategoryPosition = result.CategoryPosition,
                GenderPosition = result.GenderPosition,
                Status = result.Status,
                Splits = result.Splits,
                RecordedAt = result.RecordedAt
            };

            return CreatedAtAction(nameof(GetResultById), new { id = result.ResultId }, responseDto);
        }

        /// <summary>
        /// Updates a recorded timing result or adjusts status (Organiser only)
        /// </summary>
        [HttpPut("{id:int}")]
        [RequireRole("Organiser")]
        [SwaggerOperation(Summary = "Update Result", Description = "Organisers adjust gun/chip times or correct DQ/DNF designations.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateResult(int id, [FromBody] UpdateResultDto dto)
        {
            var result = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(en => en.Event)
                        .ThenInclude(e => e.Organiser)
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (result == null)
                return NotFound(new { message = $"Result with ID {id} not found." });

            var userCtx = HttpContext.GetCurrentUser();
            if (result.Enrolment.Event.Organiser.UserId != userCtx.UserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Forbidden. Not your event." });
            }

            result.FinishTimeSec = dto.FinishTimeSec;
            result.ChipTimeSec = dto.ChipTimeSec;
            result.GunPosition = dto.GunPosition;
            result.CategoryPosition = dto.CategoryPosition;
            result.GenderPosition = dto.GenderPosition;
            if (!string.IsNullOrWhiteSpace(dto.Status)) result.Status = dto.Status;
            result.Splits = dto.Splits;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Result updated successfully.", resultId = result.ResultId });
        }

        /// <summary>
        /// Removes a timing result row (Organiser only)
        /// </summary>
        [HttpDelete("{id:int}")]
        [RequireRole("Organiser")]
        [SwaggerOperation(Summary = "Delete Result", Description = "Removes a timing record from the leaderboard.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteResult(int id)
        {
            var result = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(en => en.Event)
                        .ThenInclude(e => e.Organiser)
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (result == null)
                return NotFound(new { message = $"Result with ID {id} not found." });

            var userCtx = HttpContext.GetCurrentUser();
            if (result.Enrolment.Event.Organiser.UserId != userCtx.UserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Forbidden. Not your event." });
            }

            _context.Results.Remove(result);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Batch captures timing mat import results matched by bib number (Organiser only)
        /// </summary>
        [HttpPost("/api/events/{eventId:int}/results/batch")]
        [RequireRole("Organiser")]
        [SwaggerOperation(Summary = "Batch Import Results", Description = "Organisers import bulk finish times matched against allocated race bib numbers.")]
        [ProducesResponseType(typeof(BatchResultResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> BatchImportResults(int eventId, [FromBody] List<BatchResultItemDto> items)
        {
            var ev = await _context.Events
                .Include(e => e.Organiser)
                .Include(e => e.Enrolments)
                    .ThenInclude(en => en.Result)
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (ev == null)
                return NotFound(new { message = $"Event with ID {eventId} not found." });

            var userCtx = HttpContext.GetCurrentUser();
            if (ev.Organiser.UserId != userCtx.UserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Forbidden. Not your event." });
            }

            var response = new BatchResultResponseDto();

            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item.BibNumber))
                {
                    response.Skipped++;
                    response.Errors.Add("Skipped entry with empty BibNumber.");
                    continue;
                }

                var enrolment = ev.Enrolments.FirstOrDefault(en => 
                    string.Equals(en.BibNumber, item.BibNumber.Trim(), StringComparison.OrdinalIgnoreCase));

                if (enrolment == null)
                {
                    response.Skipped++;
                    response.Errors.Add($"No enrolment found for BibNumber '{item.BibNumber}'.");
                    continue;
                }

                if (enrolment.Result != null)
                {
                    // Update existing
                    enrolment.Result.FinishTimeSec = item.FinishTimeSec;
                    enrolment.Result.ChipTimeSec = item.ChipTimeSec;
                    enrolment.Result.GunPosition = item.GunPosition;
                    enrolment.Result.CategoryPosition = item.CategoryPosition;
                    enrolment.Result.GenderPosition = item.GenderPosition;
                    enrolment.Result.Status = item.Status ?? "Finished";
                    enrolment.Result.Splits = item.Splits;
                    response.Updated++;
                }
                else
                {
                    // Insert new
                    var newRes = new Result
                    {
                        EnrolmentId = enrolment.EnrolmentId,
                        FinishTimeSec = item.FinishTimeSec,
                        ChipTimeSec = item.ChipTimeSec,
                        GunPosition = item.GunPosition,
                        CategoryPosition = item.CategoryPosition,
                        GenderPosition = item.GenderPosition,
                        Status = item.Status ?? "Finished",
                        Splits = item.Splits,
                        RecordedAt = DateTime.UtcNow
                    };
                    _context.Results.Add(newRes);
                    response.Inserted++;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(response);
        }

        /// <summary>
        /// Retrieves result for a specific enrolment
        /// </summary>
        [HttpGet("/api/enrolments/{enrolmentId:int}/result")]
        [SwaggerOperation(Summary = "Get Result by Enrolment ID", Description = "Endpoint plan hierarchical route to read an entrant's timing certificate.")]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetResultByEnrolment(int enrolmentId)
        {
            var r = await _context.Results
                .Include(res => res.Enrolment)
                    .ThenInclude(en => en.Participant)
                        .ThenInclude(p => p.User)
                .Include(res => res.Enrolment)
                    .ThenInclude(en => en.Event)
                .Include(res => res.Enrolment)
                    .ThenInclude(en => en.Category)
                .FirstOrDefaultAsync(res => res.EnrolmentId == enrolmentId);

            if (r == null)
                return NotFound(new { message = $"No result recorded yet for enrolment {enrolmentId}." });

            return Ok(new ResultDto
            {
                ResultId = r.ResultId,
                EnrolmentId = r.EnrolmentId,
                ParticipantName = r.Enrolment.Participant.User.FullName,
                BibNumber = r.Enrolment.BibNumber,
                EventTitle = r.Enrolment.Event.Title,
                CategoryName = r.Enrolment.Category.CategoryName,
                FinishTimeSec = r.FinishTimeSec,
                ChipTimeSec = r.ChipTimeSec,
                FinishTimeFormatted = ResultDto.FormatSeconds(r.FinishTimeSec),
                ChipTimeFormatted = ResultDto.FormatSeconds(r.ChipTimeSec),
                GunPosition = r.GunPosition,
                CategoryPosition = r.CategoryPosition,
                GenderPosition = r.GenderPosition,
                Status = r.Status,
                Splits = r.Splits,
                RecordedAt = r.RecordedAt
            });
        }
    }
}
