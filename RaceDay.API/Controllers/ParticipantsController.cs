using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Common;
using RaceDay.API.Data;
using RaceDay.API.Filters;
using RaceDay.API.Models.DTOs;
using Swashbuckle.AspNetCore.Annotations;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [RequireAuth]
    [SwaggerTag("Participant Athlete Profiles & History")]
    public class ParticipantsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ParticipantsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves complete race performance history and stats for a participant
        /// </summary>
        [HttpGet("{participantId:int}/history")]
        [SwaggerOperation(Summary = "Get Participant Race History", Description = "Returns total km completed, finish counts, and past event results for an athlete.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetParticipantHistory(int participantId)
        {
            var participant = await _context.Participants
                .Include(p => p.User)
                .Include(p => p.Enrolments)
                    .ThenInclude(en => en.Event)
                .Include(p => p.Enrolments)
                    .ThenInclude(en => en.Category)
                .Include(p => p.Enrolments)
                    .ThenInclude(en => en.Result)
                .FirstOrDefaultAsync(p => p.ParticipantId == participantId);

            if (participant == null)
                return NotFound(new { message = $"Participant with ID {participantId} not found." });

            var userCtx = HttpContext.GetCurrentUser();

            // Participants can only view their own history; Organisers can view any
            if (userCtx.Role == "Participant" && participant.UserId != userCtx.UserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Forbidden. You cannot view another athlete's race history." });
            }

            var finishedResults = participant.Enrolments
                .Where(en => en.Result != null && en.Result.Status == "Finished")
                .Select(en => new
                {
                    EventTitle = en.Event.Title,
                    CategoryName = en.Category.CategoryName,
                    DistanceKm = en.Category.DistanceKm,
                    EventDate = en.Event.StartDate,
                    FinishTimeSec = en.Result!.FinishTimeSec,
                    FinishTime = ResultDto.FormatSeconds(en.Result.FinishTimeSec),
                    GunPosition = en.Result.GunPosition,
                    CategoryPosition = en.Result.CategoryPosition
                })
                .ToList();

            decimal totalKm = finishedResults.Sum(r => r.DistanceKm);
            int totalFinishes = finishedResults.Count;

            return Ok(new
            {
                ParticipantId = participant.ParticipantId,
                AthleteName = participant.User.FullName,
                ClubName = participant.ClubName,
                TotalFinishes = totalFinishes,
                TotalKmCompleted = totalKm,
                History = finishedResults
            });
        }
    }
}
