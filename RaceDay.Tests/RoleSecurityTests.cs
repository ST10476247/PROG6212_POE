using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.Filters;
using RaceDay.API.Models.DTOs;
using RaceDay.Tests.Helpers;
using Xunit;

namespace RaceDay.Tests
{
    public class RoleSecurityTests
    {
        private readonly RaceDayDbContext _context;

        public RoleSecurityTests()
        {
            _context = TestDbContextFactory.Create();
        }

        [Fact]
        public void RequireAuthAttribute_WhenUnauthenticated_Returns401Unauthorized()
        {
            // Arrange
            var filter = new RequireAuthAttribute();
            var httpContext = TestControllerHelper.SetupHttpContext(); // unauthenticated
            var filterContext = TestControllerHelper.CreateAuthFilterContext(httpContext);

            // Act
            filter.OnAuthorization(filterContext);

            // Assert
            filterContext.Result.Should().NotBeNull();
            var json = filterContext.Result as JsonResult;
            json.Should().NotBeNull();
            json!.StatusCode.Should().Be(401);
        }

        [Fact]
        public void RequireRoleAttribute_WhenWrongRole_Returns403Forbidden()
        {
            // Arrange: Organiser role required, but caller has Participant role
            var filter = new RequireRoleAttribute("Organiser");
            var httpContext = TestControllerHelper.SetupHttpContext(userId: 2, role: "Participant");
            var filterContext = TestControllerHelper.CreateAuthFilterContext(httpContext);

            // Act
            filter.OnAuthorization(filterContext);

            // Assert
            filterContext.Result.Should().NotBeNull();
            var json = filterContext.Result as JsonResult;
            json.Should().NotBeNull();
            json!.StatusCode.Should().Be(403);
        }

        [Fact]
        public void RequireRoleAttribute_WhenUnauthenticated_Returns401Unauthorized()
        {
            // Arrange: Organiser role required, but caller is not logged in
            var filter = new RequireRoleAttribute("Organiser");
            var httpContext = TestControllerHelper.SetupHttpContext(); // no user
            var filterContext = TestControllerHelper.CreateAuthFilterContext(httpContext);

            // Act
            filter.OnAuthorization(filterContext);

            // Assert
            filterContext.Result.Should().NotBeNull();
            var json = filterContext.Result as JsonResult;
            json.Should().NotBeNull();
            json!.StatusCode.Should().Be(401);
        }

        [Fact]
        public void RequireRoleAttribute_WhenParticipantRoleRequired_RejectsOrganiserWith403()
        {
            // Arrange: Participant role required, but caller has Organiser role
            var filter = new RequireRoleAttribute("Participant");
            var httpContext = TestControllerHelper.SetupHttpContext(userId: 1, role: "Organiser");
            var filterContext = TestControllerHelper.CreateAuthFilterContext(httpContext);

            // Act
            filter.OnAuthorization(filterContext);

            // Assert
            filterContext.Result.Should().NotBeNull();
            var json = filterContext.Result as JsonResult;
            json.Should().NotBeNull();
            json!.StatusCode.Should().Be(403);
        }

        [Fact]
        public void RequireRoleAttribute_WhenCorrectRoleProvided_AllowsAccess()
        {
            // Arrange: Organiser role required, caller is Organiser
            var filter = new RequireRoleAttribute("Organiser");
            var httpContext = TestControllerHelper.SetupHttpContext(userId: 1, role: "Organiser");
            var filterContext = TestControllerHelper.CreateAuthFilterContext(httpContext);

            // Act
            filter.OnAuthorization(filterContext);

            // Assert - access allowed, Result stays null
            filterContext.Result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateEvent_WhenNonOwnerOrganiser_Returns403Forbidden()
        {
            // Arrange: Organiser 2 tries to edit Event 1 (owned by Organiser 1)
            var controller = new EventsController(_context);
            TestControllerHelper.SetOrganiser(controller, userId: 99, organiserId: 99); // different organiser

            var dto = new UpdateEventDto
            {
                Title = "Hijacked Event",
                EventType = "Running"
            };

            // Act
            var result = await controller.UpdateEvent(1, dto);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objResult = (ObjectResult)result;
            objResult.StatusCode.Should().Be(403);
        }

        [Fact]
        public async Task GetEventEnrolments_WhenNonOwnerOrganiser_Returns403Forbidden()
        {
            // Arrange: Organiser 2 tries to view Event 1's enrolments
            var controller = new EventsController(_context);
            TestControllerHelper.SetOrganiser(controller, userId: 99, organiserId: 99);

            // Act
            var result = await controller.GetEventEnrolments(1);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objResult = (ObjectResult)result;
            objResult.StatusCode.Should().Be(403);
        }
    }
}
