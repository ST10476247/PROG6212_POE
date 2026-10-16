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
    [SwaggerTag("Race Distance Categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves details of a specific race category
        /// </summary>
        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Get Category By ID", Description = "Returns category distances, start time, cutoff limit, and current entrant count.")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCategory(int id)
        {
            var cat = await _context.EventCategories
                .Include(c => c.Enrolments)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (cat == null)
                return NotFound(new { message = $"Category with ID {id} not found." });

            return Ok(new CategoryDto
            {
                CategoryId = cat.CategoryId,
                EventId = cat.EventId,
                CategoryName = cat.CategoryName,
                DistanceKm = cat.DistanceKm,
                Gender = cat.Gender,
                MinAgeYears = cat.MinAgeYears,
                MaxAgeYears = cat.MaxAgeYears,
                StartTime = cat.StartTime,
                CutoffHours = cat.CutoffHours,
                EntryFeeZar = cat.EntryFeeZar,
                MaxCapacity = cat.MaxCapacity,
                EnrolledCount = cat.Enrolments.Count
            });
        }

        /// <summary>
        /// Updates a race distance category (Organiser only)
        /// </summary>
        [HttpPut("{id:int}")]
        [RequireRole("Organiser")]
        [SwaggerOperation(Summary = "Update Category", Description = "Updates category entry fees, distances, or age boundaries.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var cat = await _context.EventCategories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (cat == null)
                return NotFound(new { message = $"Category with ID {id} not found." });

            var user = HttpContext.GetCurrentUser();
            var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserId == user.UserId);

            if (organiser == null || cat.Event.OrganiserId != organiser.OrganiserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Forbidden. You are not the owner of the event for this category." });
            }

            cat.CategoryName = dto.CategoryName.Trim();
            cat.DistanceKm = dto.DistanceKm;
            cat.Gender = dto.Gender ?? "All";
            cat.MinAgeYears = dto.MinAgeYears;
            cat.MaxAgeYears = dto.MaxAgeYears;
            cat.StartTime = dto.StartTime;
            cat.CutoffHours = dto.CutoffHours;
            cat.EntryFeeZar = dto.EntryFeeZar;
            cat.MaxCapacity = dto.MaxCapacity;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Category updated successfully.", categoryId = cat.CategoryId });
        }

        /// <summary>
        /// Removes a category if no participants are registered (Organiser only)
        /// </summary>
        [HttpDelete("{id:int}")]
        [RequireRole("Organiser")]
        [SwaggerOperation(Summary = "Delete Category", Description = "Removes a category. Refused if athlete enrolments exist.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var cat = await _context.EventCategories
                .Include(c => c.Event)
                .Include(c => c.Enrolments)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (cat == null)
                return NotFound(new { message = $"Category with ID {id} not found." });

            var user = HttpContext.GetCurrentUser();
            var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserId == user.UserId);

            if (organiser == null || cat.Event.OrganiserId != organiser.OrganiserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Forbidden. You are not the owner of this event." });
            }

            if (cat.Enrolments.Any())
            {
                return Conflict(new { message = "Cannot delete category because participants have already enrolled into it." });
            }

            _context.EventCategories.Remove(cat);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
