using System;
using System.Collections.Generic;
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
    public class EnrolmentTests
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentTests()
        {
            _context = TestDbContextFactory.Create();
        }

        [Fact]
        public async Task EnrolParticipant_WithValidCategory_AllocatesBibAndPaymentReference()
        {
            // Arrange
            var controller = new EnrolmentsController(_context);
            TestControllerHelper.SetParticipant(controller, userId: 2, participantId: 1);

            var dto = new CreateEnrolmentDto
            {
                CategoryId = 1,
                EmergencyNotes = "No known medical allergies."
            };

            // Act
            var result = await controller.EnrolParticipant(dto);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var created = (CreatedAtActionResult)result;
            var response = created.Value as EnrolmentResponseDto;
            response.Should().NotBeNull();
            response!.BibNumber.Should().NotBeNullOrWhiteSpace();
            response.PaymentReference.Should().StartWith("PAY-1-1-");
            response.PaymentStatus.Should().Be("Pending");
            response.Status.Should().Be("Registered");
            response.CategoryName.Should().Be("21.1km Half Marathon");
        }

        [Fact]
        public async Task EnrolParticipant_WhenAlreadyEnrolled_Returns409Conflict()
        {
            // Arrange
            var controller = new EnrolmentsController(_context);
            TestControllerHelper.SetParticipant(controller, userId: 2, participantId: 1);

            var dto = new CreateEnrolmentDto
            {
                CategoryId = 1,
                EmergencyNotes = "First Entry"
            };

            // First enrolment succeeds
            await controller.EnrolParticipant(dto);

            // Act - attempt duplicate enrolment into the same category
            var secondResult = await controller.EnrolParticipant(dto);

            // Assert
            secondResult.Should().BeOfType<ConflictObjectResult>();
            var conflict = (ConflictObjectResult)secondResult;
            conflict.StatusCode.Should().Be(409);
        }

        [Fact]
        public async Task EnrolParticipant_WhenCategoryDoesNotExist_Returns404NotFound()
        {
            // Arrange
            var controller = new EnrolmentsController(_context);
            TestControllerHelper.SetParticipant(controller, userId: 2, participantId: 1);

            var dto = new CreateEnrolmentDto
            {
                CategoryId = 99999 // Non-existent category
            };

            // Act
            var result = await controller.EnrolParticipant(dto);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetMyEnrolments_ReturnsParticipantEnrolmentHistory()
        {
            // Arrange: Add an enrolment for participant 1
            var enrolment = new EventEnrolment
            {
                ParticipantId = 1,
                EventId = 1,
                CategoryId = 2,
                BibNumber = "T-1002",
                PaymentStatus = "Paid",
                Status = "Registered"
            };
            _context.EventEnrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            var controller = new EnrolmentsController(_context);
            TestControllerHelper.SetParticipant(controller, userId: 2, participantId: 1);

            // Act
            var result = await controller.GetMyEnrolments();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var ok = (OkObjectResult)result;
            var list = ok.Value as List<EnrolmentResponseDto>;
            list.Should().NotBeNull();
            list.Should().Contain(en => en.BibNumber == "T-1002");
        }

        [Fact]
        public async Task GetEnrolmentById_WhenAccessingOtherParticipantTicket_Returns403Forbidden()
        {
            // Arrange: Enrolment belongs to Participant 1 (UserId 2)
            var enrolment = new EventEnrolment
            {
                ParticipantId = 1,
                EventId = 1,
                CategoryId = 1,
                BibNumber = "P-1001",
                PaymentStatus = "Paid",
                Status = "Registered"
            };
            _context.EventEnrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            // Caller is a different Participant (UserId 99, ParticipantId 99)
            var controller = new EnrolmentsController(_context);
            TestControllerHelper.SetParticipant(controller, userId: 99, participantId: 99);

            // Act
            var result = await controller.GetEnrolmentById(enrolment.EnrolmentId);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objResult = (ObjectResult)result;
            objResult.StatusCode.Should().Be(403);
        }
    }
}
