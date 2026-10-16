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
    [SwaggerTag("Road Events Management & Race Details")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Public listing of South African road events with filtering and pagination
        /// </summary>
        [HttpGet]
        [SwaggerOperation(Summary = "List Events", Description = "Returns a paged list of road running, cycling, and walking events with category and entrant counts.")]
        [ProducesResponseType(typeof(PagedResultDto<EventSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEvents([FromQuery] EventQueryParameters q)
        {
            var query = _context.Events
                .Include(e => e.Organiser)
                .Include(e => e.Categories)
                .Include(e => e.Enrolments)
                .AsNoTracking();

            var user = HttpContext.GetCurrentUser();

            // Participants and public callers only see Open, Closed, or Completed events; Organisers see their own Drafts too
            if (user.Role != "Organiser")
            {
                query = query.Where(e => e.Status == "Open" || e.Status == "Closed" || e.Status == "Completed");
            }

            if (!string.IsNullOrWhiteSpace(q.City))
                query = query.Where(e => e.City.ToLower().Contains(q.City.Trim().ToLower()));

            if (!string.IsNullOrWhiteSpace(q.Province))
                query = query.Where(e => e.Province.ToLower().Contains(q.Province.Trim().ToLower()));

            if (!string.IsNullOrWhiteSpace(q.EventType))
                query = query.Where(e => e.EventType.ToLower() == q.EventType.Trim().ToLower());

            if (!string.IsNullOrWhiteSpace(q.Status))
                query = query.Where(e => e.Status.ToLower() == q.Status.Trim().ToLower());

            if (!string.IsNullOrWhiteSpace(q.Search))
            {
                string s = q.Search.Trim().ToLower();
                query = query.Where(e => e.Title.ToLower().Contains(s) || (e.Description != null && e.Description.ToLower().Contains(s)));
            }

            if (q.From.HasValue)
                query = query.Where(e => e.StartDate >= q.From.Value);

            if (q.To.HasValue)
                query = query.Where(e => e.StartDate <= q.To.Value);

            int totalCount = await query.CountAsync();

            int page = Math.Max(1, q.Page);
            int pageSize = Math.Clamp(q.PageSize, 1, 100);

            var items = await query
                .OrderBy(e => e.StartDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EventSummaryDto
                {
                    EventId = e.EventId,
                    OrganiserId = e.OrganiserId,
                    OrganiserName = e.Organiser.CompanyName,
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

            return Ok(new PagedResultDto<EventSummaryDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        /// <summary>
        /// Retrieves featured major South African road events (Comrades, Cape Town Cycle Tour, Soweto Marathon)
        /// </summary>
        [HttpGet("featured")]
        [SwaggerOperation(Summary = "Featured Events", Description = "Retrieves high-profile iconic South African sporting events.")]
        [ProducesResponseType(typeof(List<EventSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFeaturedEvents()
        {
            var featured = await _context.Events
                .Include(e => e.Organiser)
                .Include(e => e.Categories)
                .Include(e => e.Enrolments)
                .Where(e => e.Status == "Open" || e.Status == "Upcoming")
                .OrderByDescending(e => e.MaxEntries)
                .Take(6)
                .Select(e => new EventSummaryDto
                {
                    EventId = e.EventId,
                    OrganiserId = e.OrganiserId,
                    OrganiserName = e.Organiser.CompanyName,
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

            return Ok(featured);
        }

        /// <summary>
        /// Filters events by query parameters (alias for search)
        /// </summary>
        [HttpGet("search")]
        [SwaggerOperation(Summary = "Search Events", Description = "Search events by province, sport type, and keyword.")]
        public Task<IActionResult> SearchEvents([FromQuery] EventQueryParameters q)
        {
            return GetEvents(q);
        }

        /// <summary>
        /// Retrieves complete details of a single event including categories and live weather
        /// </summary>
        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Get Event By ID", Description = "Returns full event details with available distance categories and weather preview.")]
        [ProducesResponseType(typeof(EventDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEventById(int id)
        {
            var e = await _context.Events
                .Include(e => e.Organiser)
                .Include(e => e.Categories)
                    .ThenInclude(c => c.Enrolments)
                .Include(e => e.Enrolments)
                .Include(e => e.WeatherSnapshots)
                .FirstOrDefaultAsync(ev => ev.EventId == id);

            if (e == null)
                return NotFound(new { message = $"Event with ID {id} was not found." });

            var dto = new EventDetailDto
            {
                EventId = e.EventId,
                OrganiserId = e.OrganiserId,
                OrganiserName = e.Organiser.CompanyName,
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
                EntrantsCount = e.Enrolments.Count,
                RouteUrl = e.RouteUrl,
                ElevationGainMeters = e.ElevationGainMeters,
                KeyLandmarks = e.KeyLandmarks,
                TerrainType = e.TerrainType,
                RegistrationOpen = e.RegistrationOpen,
                RegistrationClose = e.RegistrationClose,
                Categories = e.Categories.Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    EventId = c.EventId,
                    CategoryName = c.CategoryName,
                    DistanceKm = c.DistanceKm,
                    Gender = c.Gender,
                    MinAgeYears = c.MinAgeYears,
                    MaxAgeYears = c.MaxAgeYears,
                    StartTime = c.StartTime,
                    CutoffHours = c.CutoffHours,
                    EntryFeeZar = c.EntryFeeZar,
                    MaxCapacity = c.MaxCapacity,
                    EnrolledCount = c.Enrolments.Count
                }).ToList(),
                WeatherSnapshots = e.WeatherSnapshots.OrderByDescending(w => w.SnapshotTime).Take(5).Select(w => new WeatherSnapshotDto
                {
                    SnapshotId = w.SnapshotId,
                    EventId = w.EventId,
                    SnapshotTime = w.SnapshotTime,
                    TemperatureC = w.TemperatureC,
                    HumidityPct = w.HumidityPct,
                    WindKmh = w.WindKmh,
                    Condition = w.Condition,
                    Source = w.Source,
                    RaceDayAdvice = w.RaceDayAdvice
                }).ToList()
            };

            return Ok(dto);
        }

        /// <summary>
        /// Creates a new road running, walking, or cycling event (Organiser only)
        /// </summary>
        [HttpPost]
        [RequireRole("Organiser")]
        [SwaggerOperation(Summary = "Create Event", Description = "Organisers create a new road event in their organisation.")]
        [ProducesResponseType(typeof(EventDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = HttpContext.GetCurrentUser();

            // Find organiser profile linked to user
            var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserId == user.UserId);
            if (organiser == null)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Organiser profile not found for current user." });
            }

            var ev = new Event
            {
                OrganiserId = organiser.OrganiserId,
                Title = dto.Title.Trim(),
                EventType = dto.EventType,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                City = dto.City.Trim(),
                Province = dto.Province.Trim(),
                RouteUrl = dto.RouteUrl,
                EntryFeeZar = dto.EntryFeeZar,
                MaxEntries = dto.MaxEntries,
                RegistrationOpen = dto.RegistrationOpen,
                RegistrationClose = dto.RegistrationClose,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Open" : dto.Status,
                ElevationGainMeters = dto.ElevationGainMeters,
                KeyLandmarks = dto.KeyLandmarks,
                TerrainType = dto.TerrainType ?? "Road / Tarmac",
                BannerUrl = dto.BannerUrl
            };

            _context.Events.Add(ev);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventById), new { id = ev.EventId }, new { eventId = ev.EventId, message = "Event created successfully.", title = ev.Title });
        }

        /// <summary>
        /// Updates an existing event (Organiser owner only)
        /// </summary>
        [HttpPut("{id:int}")]
        [RequireRole("Organiser")]
        [SwaggerOperation(Summary = "Update Event", Description = "Updates event metadata, location, dates, or distance descriptions.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ev = await _context.Events.FindAsync(id);
            if (ev == null)
                return NotFound(new { message = $"Event with ID {id} not found." });

            var user = HttpContext.GetCurrentUser();
            var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserId == user.UserId);

            if (organiser == null || ev.OrganiserId != organiser.OrganiserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Forbidden. You are not the owner of this event." });
            }

            ev.Title = dto.Title.Trim();
            ev.EventType = dto.EventType;
            ev.Description = dto.Description;
            ev.StartDate = dto.StartDate;
            ev.EndDate = dto.EndDate;
            ev.City = dto.City.Trim();
            ev.Province = dto.Province.Trim();
            ev.RouteUrl = dto.RouteUrl;
            ev.EntryFeeZar = dto.EntryFeeZar;
            ev.MaxEntries = dto.MaxEntries;
            ev.RegistrationOpen = dto.RegistrationOpen;
            ev.RegistrationClose = dto.RegistrationClose;
            if (!string.IsNullOrWhiteSpace(dto.Status)) ev.Status = dto.Status;
            ev.ElevationGainMeters = dto.ElevationGainMeters;
            ev.KeyLandmarks = dto.KeyLandmarks;
            ev.TerrainType = dto.TerrainType;
            ev.BannerUrl = dto.BannerUrl;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Event updated successfully.", eventId = ev.EventId });
        }

        /// <summary>
        /// Deletes an event (Organiser owner only; rejected if paid enrolments exist)
        /// </summary>
        [HttpDelete("{id:int}")]
        [RequireRole("Organiser")]
        [SwaggerOperation(Summary = "Delete Event", Description = "Removes an event. Returns 409 Conflict if active paid enrolments exist.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Enrolments)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null)
                return NotFound(new { message = $"Event with ID {id} not found." });

            var user = HttpContext.GetCurrentUser();
            var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserId == user.UserId);

            if (organiser == null || ev.OrganiserId != organiser.OrganiserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Forbidden. You are not the owner of this event." });
            }

            if (ev.Enrolments.Any(en => en.PaymentStatus == "Paid"))
            {
                return Conflict(new { message = "Cannot delete this event because active paid participant enrolments exist. Please issue refunds or mark event status as 'Cancelled' instead." });
            }

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Lists all distance categories configured for an event
        /// </summary>
        [HttpGet("{id:int}/categories")]
        [SwaggerOperation(Summary = "Get Event Categories", Description = "Lists all race categories under this event (e.g. 89km, 42.2km, 21.1km).")]
        [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategories(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null)
                return NotFound(new { message = $"Event with ID {id} not found." });

            var categories = await _context.EventCategories
                .Where(c => c.EventId == id)
                .Include(c => c.Enrolments)
                .Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    EventId = c.EventId,
                    CategoryName = c.CategoryName,
                    DistanceKm = c.DistanceKm,
                    Gender = c.Gender,
                    MinAgeYears = c.MinAgeYears,
                    MaxAgeYears = c.MaxAgeYears,
                    StartTime = c.StartTime,
                    CutoffHours = c.CutoffHours,
                    EntryFeeZar = c.EntryFeeZar,
                    MaxCapacity = c.MaxCapacity,
                    EnrolledCount = c.Enrolments.Count
                })
                .ToListAsync();

            return Ok(categories);
        }

        /// <summary>
        /// Adds a new distance category to an event (Organiser only)
        /// </summary>
        [HttpPost("{id:int}/categories")]
        [RequireRole("Organiser")]
        [SwaggerOperation(Summary = "Add Category to Event", Description = "Creates a race distance category under the event.")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AddCategory(int id, [FromBody] CreateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ev = await _context.Events.FindAsync(id);
            if (ev == null)
                return NotFound(new { message = $"Event with ID {id} not found." });

            var user = HttpContext.GetCurrentUser();
            var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserId == user.UserId);

            if (organiser == null || ev.OrganiserId != organiser.OrganiserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Forbidden. You are not the owner of this event." });
            }

            if (await _context.EventCategories.AnyAsync(c => c.EventId == id && c.CategoryName.ToLower() == dto.CategoryName.Trim().ToLower()))
            {
                return Conflict(new { message = $"Category '{dto.CategoryName}' already exists for this event." });
            }

            var cat = new EventCategory
            {
                EventId = id,
                CategoryName = dto.CategoryName.Trim(),
                DistanceKm = dto.DistanceKm,
                Gender = dto.Gender ?? "All",
                MinAgeYears = dto.MinAgeYears,
                MaxAgeYears = dto.MaxAgeYears,
                StartTime = dto.StartTime,
                CutoffHours = dto.CutoffHours,
                EntryFeeZar = dto.EntryFeeZar,
                MaxCapacity = dto.MaxCapacity
            };

            _context.EventCategories.Add(cat);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategories), new { id = ev.EventId }, new CategoryDto
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
                EnrolledCount = 0
            });
        }

        /// <summary>
        /// Retrieves participant enrolment roster for an event (Organiser only)
        /// </summary>
        [HttpGet("{id:int}/enrolments")]
        [RequireRole("Organiser")]
        [SwaggerOperation(Summary = "Get Event Enrolments", Description = "Organisers view the list of registered athletes and allocated bibs for their event.")]
        [ProducesResponseType(typeof(List<EnrolmentResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEventEnrolments(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null)
                return NotFound(new { message = $"Event with ID {id} not found." });

            var user = HttpContext.GetCurrentUser();
            var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserId == user.UserId);

            if (organiser == null || ev.OrganiserId != organiser.OrganiserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Forbidden. You can only view enrolments for your own events." });
            }

            var enrolments = await _context.EventEnrolments
                .Include(en => en.Participant)
                    .ThenInclude(p => p.User)
                .Include(en => en.Category)
                .Include(en => en.Result)
                .Where(en => en.EventId == id)
                .OrderBy(en => en.EnrolmentDate)
                .Select(en => new EnrolmentResponseDto
                {
                    EnrolmentId = en.EnrolmentId,
                    ParticipantId = en.ParticipantId,
                    ParticipantName = en.Participant.User.FullName,
                    ParticipantEmail = en.Participant.User.Email,
                    EventId = en.EventId,
                    EventTitle = ev.Title,
                    CategoryId = en.CategoryId,
                    CategoryName = en.Category.CategoryName,
                    DistanceKm = en.Category.DistanceKm,
                    EnrolmentDate = en.EnrolmentDate,
                    BibNumber = en.BibNumber,
                    PaymentStatus = en.PaymentStatus,
                    PaymentReference = en.PaymentReference,
                    Status = en.Status,
                    EmergencyNotes = en.EmergencyNotes
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        /// <summary>
        /// Public leaderboard and timing results for an event
        /// </summary>
        [HttpGet("{id:int}/results")]
        [SwaggerOperation(Summary = "Get Event Results Leaderboard", Description = "Public official finish times, chip times, and rankings for an event.")]
        [ProducesResponseType(typeof(List<ResultDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEventResults(int id, [FromQuery] int? categoryId)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null)
                return NotFound(new { message = $"Event with ID {id} not found." });

            var query = _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(en => en.Participant)
                        .ThenInclude(p => p.User)
                .Include(r => r.Enrolment)
                    .ThenInclude(en => en.Category)
                .Where(r => r.Enrolment.EventId == id);

            if (categoryId.HasValue)
            {
                query = query.Where(r => r.Enrolment.CategoryId == categoryId.Value);
            }

            var results = await query
                .OrderBy(r => r.GunPosition ?? int.MaxValue)
                .ThenBy(r => r.ChipTimeSec ?? int.MaxValue)
                .Select(r => new ResultDto
                {
                    ResultId = r.ResultId,
                    EnrolmentId = r.EnrolmentId,
                    ParticipantName = r.Enrolment.Participant.User.FullName,
                    BibNumber = r.Enrolment.BibNumber,
                    EventTitle = ev.Title,
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
        /// Retrieves weather history and forecasts for an event
        /// </summary>
        [HttpGet("{id:int}/weather")]
        [SwaggerOperation(Summary = "Get Event Weather", Description = "Returns real-time weather readings and race day hydration advice.")]
        [ProducesResponseType(typeof(List<WeatherSnapshotDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEventWeather(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null)
                return NotFound(new { message = $"Event with ID {id} not found." });

            var snapshots = await _context.WeatherSnapshots
                .Where(w => w.EventId == id)
                .OrderByDescending(w => w.SnapshotTime)
                .Select(w => new WeatherSnapshotDto
                {
                    SnapshotId = w.SnapshotId,
                    EventId = w.EventId,
                    SnapshotTime = w.SnapshotTime,
                    TemperatureC = w.TemperatureC,
                    HumidityPct = w.HumidityPct,
                    WindKmh = w.WindKmh,
                    Condition = w.Condition,
                    Source = w.Source,
                    RaceDayAdvice = w.RaceDayAdvice
                })
                .ToListAsync();

            return Ok(snapshots);
        }

        /// <summary>
        /// Captures a live weather snapshot for an event (Organiser only)
        /// </summary>
        [HttpPost("{id:int}/weather")]
        [RequireRole("Organiser")]
        [SwaggerOperation(Summary = "Capture Weather Snapshot", Description = "Organisers record race morning temperature, wind, and hydration recommendations.")]
        [ProducesResponseType(typeof(WeatherSnapshotDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> CaptureWeather(int id, [FromBody] CreateWeatherSnapshotDto dto)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null)
                return NotFound(new { message = $"Event with ID {id} not found." });

            var snapshot = new WeatherSnapshot
            {
                EventId = id,
                SnapshotTime = DateTime.UtcNow,
                TemperatureC = dto.TemperatureC,
                HumidityPct = dto.HumidityPct,
                WindKmh = dto.WindKmh,
                Condition = dto.Condition ?? "Mild",
                Source = dto.Source ?? "Race Operations",
                RaceDayAdvice = dto.RaceDayAdvice ?? "Ensure proper fluid balance and wear sunscreen."
            };

            _context.WeatherSnapshots.Add(snapshot);
            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, new WeatherSnapshotDto
            {
                SnapshotId = snapshot.SnapshotId,
                EventId = snapshot.EventId,
                SnapshotTime = snapshot.SnapshotTime,
                TemperatureC = snapshot.TemperatureC,
                HumidityPct = snapshot.HumidityPct,
                WindKmh = snapshot.WindKmh,
                Condition = snapshot.Condition,
                Source = snapshot.Source,
                RaceDayAdvice = snapshot.RaceDayAdvice
            });
        }

        /// <summary>
        /// Retrieves course route elevation profile and landmarks
        /// </summary>
        [HttpGet("{id:int}/route")]
        [SwaggerOperation(Summary = "Get Event Route Profile", Description = "Returns elevation gain, terrain type, and key landmarks (e.g. Polly Shorts, Chapman's Peak).")]
        [ProducesResponseType(typeof(RouteInfoDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEventRoute(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null)
                return NotFound(new { message = $"Event with ID {id} not found." });

            return Ok(new RouteInfoDto
            {
                EventId = ev.EventId,
                EventTitle = ev.Title,
                City = ev.City,
                Province = ev.Province,
                ElevationGainMeters = ev.ElevationGainMeters,
                KeyLandmarks = ev.KeyLandmarks,
                TerrainType = ev.TerrainType,
                RouteUrl = ev.RouteUrl
            });
        }
    }
}
