using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.Models.DTOs;
using RaceDay.API.Models.Entities;
using RaceDay.Tests.Helpers;
using Xunit;

namespace RaceDay.Tests
{
    public class EventManagementTests
    {
        private readonly RaceDayDbContext _context;

        public EventManagementTests()
        {
            _context = TestDbContextFactory.Create();
        }

        [Fact]
        public async Task GetEvents_PublicAccess_ReturnsOkAndPagedEvents()
        {
            // Arrange
            var controller = new EventsController(_context);
            TestControllerHelper.SetAnonymous(controller);

            // Act
            var result = await controller.GetEvents(new EventQueryParameters());

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var ok = (OkObjectResult)result;
            var paged = ok.Value as PagedResultDto<EventSummaryDto>;
            paged.Should().NotBeNull();
            paged!.TotalCount.Should().BeGreaterThanOrEqualTo(1);
            paged.Items.Should().Contain(e => e.Title == "Durban City 10km & 21km");
        }

        [Fact]
        public async Task GetEventById_ExistingId_ReturnsFullEventWithCategories()
        {
            // Arrange
            var controller = new EventsController(_context);
            TestControllerHelper.SetAnonymous(controller);

            // Act
            var result = await controller.GetEventById(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var ok = (OkObjectResult)result;
            var detail = ok.Value as EventDetailDto;
            detail.Should().NotBeNull();
            detail!.EventId.Should().Be(1);
            detail.Categories.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetEventById_NonExistingId_Returns404NotFound()
        {
            // Arrange
            var controller = new EventsController(_context);
            TestControllerHelper.SetAnonymous(controller);

            // Act
            var result = await controller.GetEventById(9999);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task CreateEvent_WhenOrganiser_Returns201Created()
        {
            // Arrange
            var controller = new EventsController(_context);
            TestControllerHelper.SetOrganiser(controller, userId: 1, organiserId: 1);

            var dto = new CreateEventDto
            {
                Title = "Pretoria City Half Marathon",
                EventType = "Running",
                Description = "Challenging altitude half marathon around the Union Buildings.",
                StartDate = DateTime.UtcNow.AddMonths(4),
                City = "Pretoria",
                Province = "Gauteng",
                EntryFeeZar = 300.00m,
                MaxEntries = 8000,
                ElevationGainMeters = 320,
                TerrainType = "Road / Hills"
            };

            // Act
            var result = await controller.CreateEvent(dto);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var created = (CreatedAtActionResult)result;
            created.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task UpdateEvent_WhenOwnerOrganiser_Returns200Ok()
        {
            // Arrange
            var controller = new EventsController(_context);
            TestControllerHelper.SetOrganiser(controller, userId: 1, organiserId: 1);

            var dto = new UpdateEventDto
            {
                Title = "Durban City 10km & 21km - Updated 2026",
                EventType = "Running",
                Description = "Updated route description along Moses Mabhida Stadium.",
                StartDate = DateTime.UtcNow.AddMonths(2),
                City = "Durban",
                Province = "KwaZulu-Natal",
                EntryFeeZar = 280.00m,
                MaxEntries = 6000
            };

            // Act
            var result = await controller.UpdateEvent(1, dto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var updatedEv = await _context.Events.FindAsync(1);
            updatedEv!.Title.Should().Be("Durban City 10km & 21km - Updated 2026");
            updatedEv.EntryFeeZar.Should().Be(280.00m);
        }

        [Fact]
        public async Task DeleteEvent_WhenNoPaidEntries_Returns204NoContent()
        {
            // Arrange
            var controller = new EventsController(_context);
            TestControllerHelper.SetOrganiser(controller, userId: 1, organiserId: 1);

            // Create a disposable event
            var ev = new Event
            {
                OrganiserId = 1,
                Title = "Disposable Event",
                EventType = "Walking",
                StartDate = DateTime.UtcNow.AddMonths(1),
                City = "Bloemfontein",
                Province = "Free State"
            };
            _context.Events.Add(ev);
            await _context.SaveChangesAsync();

            // Act
            var result = await controller.DeleteEvent(ev.EventId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            var deleted = await _context.Events.FindAsync(ev.EventId);
            deleted.Should().BeNull();
        }

        [Fact]
        public async Task DeleteEvent_WhenActivePaidEntriesExist_Returns409Conflict()
        {
            // Arrange
            var controller = new EventsController(_context);
            TestControllerHelper.SetOrganiser(controller, userId: 1, organiserId: 1);

            // Add an active paid entry to Event 1
            var enrolment = new EventEnrolment
            {
                ParticipantId = 1,
                EventId = 1,
                CategoryId = 1,
                BibNumber = "A-1001",
                PaymentStatus = "Paid",
                Status = "Registered"
            };
            _context.EventEnrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            // Act
            var result = await controller.DeleteEvent(1);

            // Assert - protect against accidental event deletion with registered paying athletes
            result.Should().BeOfType<ConflictObjectResult>();
            var conflict = (ConflictObjectResult)result;
            conflict.StatusCode.Should().Be(409);
        }
    }
}
