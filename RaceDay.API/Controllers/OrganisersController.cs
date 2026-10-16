using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Filters;
using RaceDay.API.Models.DTOs;
using Swashbuckle.AspNetCore.Annotations;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [SwaggerTag("Organiser Management & Event Stats")]
    public class OrganisersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public OrganisersController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lists all events owned by a specific organiser (Organiser dashboard)
        /// </summary>
        [HttpGet("{organiserId:int}/events")]
        [RequireRole("Organiser")]
        [SwaggerOperation(Summary = "Get Organiser Events", Description = "Organisers retrieve all events they host with category and entrant totals.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrganiserEvents(int organiserId)
        {
            var events = await _context.Events
                .Include(e => e.Categories)
                .Include(e => e.Enrolments)
                .Where(e => e.OrganiserId == organiserId)
                .OrderByDescending(e => e.StartDate)
                .Select(e => new EventSummaryDto
                {
                    EventId = e.EventId,
                    OrganiserId = e.OrganiserId,
                    Title = e.Title,
                    EventType = e.EventType,
                    Description = e.Description,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    City = e.City,
                    Province = e.Province,
                    EntryFeeZar = e.EntryFeeZar,
                    MaxEntries = e.MaxEntries,
                    Status = e.Status,
                    BannerUrl = e.BannerUrl,
                    CategoriesCount = e.Categories.Count,
                    EntrantsCount = e.Enrolments.Count
                })
                .ToListAsync();

            return Ok(events);
        }
    }
}
