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
    public class ResultTests
    {
        private readonly RaceDayDbContext _context;

        public ResultTests()
        {
            _context = TestDbContextFactory.Create();
        }

        [Fact]
        public async Task CaptureResult_WhenOrganiser_RecordsFinishTimesAndRankings()
        {
            // Arrange: Add an enrolment to Event 1
            var enrolment = new EventEnrolment
            {
                ParticipantId = 1,
                EventId = 1,
                CategoryId = 1,
                BibNumber = "R-1092",
                PaymentStatus = "Paid",
                Status = "Finished"
            };
            _context.EventEnrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            var controller = new ResultsController(_context);
            TestControllerHelper.SetOrganiser(controller, userId: 1, organiserId: 1);

            var dto = new CreateResultDto
            {
                EnrolmentId = enrolment.EnrolmentId,
                FinishTimeSec = 5400, // 1h 30m 00s
                ChipTimeSec = 5380,   // 1h 29m 40s
                GunPosition = 14,
                CategoryPosition = 3,
                GenderPosition = 12,
                Status = "Finished",
                Splits = "10k:00:42:15, 15k:01:04:30"
            };

            // Act
            var result = await controller.CaptureResult(dto);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var created = (CreatedAtActionResult)result;
            var response = created.Value as ResultDto;
            response.Should().NotBeNull();
            response!.FinishTimeFormatted.Should().Be("01:30:00");
            response.ChipTimeFormatted.Should().Be("01:29:40");
            response.GunPosition.Should().Be(14);
            response.Status.Should().Be("Finished");
        }

        [Fact]
        public async Task CaptureResult_WhenDuplicateForSameEnrolment_Returns409Conflict()
        {
            // Arrange
            var enrolment = new EventEnrolment
            {
                ParticipantId = 1,
                EventId = 1,
                CategoryId = 1,
                BibNumber = "R-1093",
                PaymentStatus = "Paid",
                Status = "Finished"
            };
            _context.EventEnrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            var controller = new ResultsController(_context);
            TestControllerHelper.SetOrganiser(controller, userId: 1, organiserId: 1);

            var dto = new CreateResultDto
            {
                EnrolmentId = enrolment.EnrolmentId,
                FinishTimeSec = 5400,
                Status = "Finished"
            };

            // First capture
            await controller.CaptureResult(dto);

            // Act: duplicate capture attempt
            var duplicate = await controller.CaptureResult(dto);

            // Assert
            duplicate.Should().BeOfType<ConflictObjectResult>();
            var conflict = (ConflictObjectResult)duplicate;
            conflict.StatusCode.Should().Be(409);
        }

        [Fact]
        public async Task BatchImportResults_WithValidBibs_UpsertsSuccessfully()
        {
            // Arrange: Seed two enrolments with bib numbers
            var en1 = new EventEnrolment
            {
                ParticipantId = 1,
                EventId = 1,
                CategoryId = 1,
                BibNumber = "BIB-101",
                PaymentStatus = "Paid",
                Status = "Registered"
            };

            _context.EventEnrolments.Add(en1);
            await _context.SaveChangesAsync();

            var controller = new ResultsController(_context);
            TestControllerHelper.SetOrganiser(controller, userId: 1, organiserId: 1);

            var batchItems = new List<BatchResultItemDto>
            {
                new BatchResultItemDto
                {
                    BibNumber = "BIB-101",
                    FinishTimeSec = 4920,
                    ChipTimeSec = 4910,
                    GunPosition = 5,
                    Status = "Finished"
                },
                new BatchResultItemDto
                {
                    BibNumber = "NON-EXISTENT-BIB",
                    FinishTimeSec = 6000,
                    Status = "Finished"
                }
            };

            // Act
            var result = await controller.BatchImportResults(1, batchItems);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var ok = (OkObjectResult)result;
            var response = ok.Value as BatchResultResponseDto;
            response.Should().NotBeNull();
            response!.Inserted.Should().Be(1);
            response.Skipped.Should().Be(1); // non-existent bib skipped
        }

        [Fact]
        public async Task GetMyResults_ReturnsOnlyParticipantFinishes()
        {
            // Arrange: Add enrolment and result for Participant 1
            var en = new EventEnrolment
            {
                ParticipantId = 1,
                EventId = 1,
                CategoryId = 1,
                BibNumber = "MY-BIB-1",
                PaymentStatus = "Paid",
                Status = "Finished"
            };
            _context.EventEnrolments.Add(en);
            await _context.SaveChangesAsync();

            var res = new Result
            {
                EnrolmentId = en.EnrolmentId,
                FinishTimeSec = 3600,
                GunPosition = 1,
                Status = "Finished"
            };
            _context.Results.Add(res);
            await _context.SaveChangesAsync();

            var controller = new ResultsController(_context);
            TestControllerHelper.SetParticipant(controller, userId: 2, participantId: 1);

            // Act
            var result = await controller.GetMyResults();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var ok = (OkObjectResult)result;
            var list = ok.Value as List<ResultDto>;
            list.Should().NotBeNull();
            list.Should().Contain(r => r.BibNumber == "MY-BIB-1");
        }
    }
}
