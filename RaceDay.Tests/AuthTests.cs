using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.Models.DTOs;
using RaceDay.API.Services;
using RaceDay.Tests.Helpers;
using Xunit;

namespace RaceDay.Tests
{
    public class AuthTests
    {
        private readonly RaceDayDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;

        public AuthTests()
        {
            _context = TestDbContextFactory.Create();
            _passwordHasher = new PasswordHasher();

            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["Jwt:SecretKey"]).Returns("TestSecretKeyForUnitTestingPurposesOnly2026!LongEnough");
            configMock.Setup(c => c["Jwt:Issuer"]).Returns("RaceDayTest");
            configMock.Setup(c => c["Jwt:Audience"]).Returns("RaceDayTestAudience");
            configMock.Setup(c => c["Jwt:ExpiryMinutes"]).Returns("60");

            _tokenService = new TokenService(configMock.Object);
        }

        [Fact]
        public async Task RegisterParticipant_WithValidData_Returns201AndToken()
        {
            // Arrange
            var controller = new AuthController(_context, _passwordHasher, _tokenService);
            TestControllerHelper.SetAnonymous(controller);

            var dto = new RegisterParticipantDto
            {
                Email = "newrunner@raceday.co.za",
                Password = "Password123!",
                FullName = "Kagiso Rabada",
                DateOfBirth = new DateTime(1995, 5, 25),
                Gender = "Male",
                SAIDOrPassport = "9505255800089",
                ClubName = "Pirates Road Running",
                EmergencyContactName = "Mama Rabada",
                EmergencyContactNo = "+27 82 000 1111",
                TshirtSize = "XL"
            };

            // Act
            var result = await controller.RegisterParticipant(dto);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var created = (CreatedAtActionResult)result;
            var response = created.Value as AuthResponseDto;
            response.Should().NotBeNull();
            response!.Email.Should().Be("newrunner@raceday.co.za");
            response.Role.Should().Be("Participant");
            response.Token.Should().NotBeNullOrWhiteSpace();
            response.ParticipantId.Should().BePositive();
        }

        [Fact]
        public async Task RegisterParticipant_WithDuplicateEmail_Returns409Conflict()
        {
            // Arrange - runner@raceday.co.za was seeded
            var controller = new AuthController(_context, _passwordHasher, _tokenService);
            TestControllerHelper.SetAnonymous(controller);

            var dto = new RegisterParticipantDto
            {
                Email = "runner@raceday.co.za", // duplicate!
                Password = "Password123!",
                FullName = "Another Runner",
                DateOfBirth = new DateTime(1990, 1, 1)
            };

            // Act
            var result = await controller.RegisterParticipant(dto);

            // Assert
            result.Should().BeOfType<ConflictObjectResult>();
            var conflict = (ConflictObjectResult)result;
            conflict.StatusCode.Should().Be(409);
        }

        [Fact]
        public async Task RegisterOrganiser_WithValidData_Returns201AndOrganiserId()
        {
            // Arrange
            var controller = new AuthController(_context, _passwordHasher, _tokenService);
            TestControllerHelper.SetAnonymous(controller);

            var dto = new RegisterOrganiserDto
            {
                Email = "neworganiser@raceday.co.za",
                Password = "Password123!",
                FullName = "Bongani Zungu",
                CompanyName = "Cape Events Management",
                CompanyRegNo = "2020/555555/07",
                ContactEmail = "info@capeevents.co.za"
            };

            // Act
            var result = await controller.RegisterOrganiser(dto);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var created = (CreatedAtActionResult)result;
            var response = created.Value as AuthResponseDto;
            response.Should().NotBeNull();
            response!.Role.Should().Be("Organiser");
            response.OrganiserId.Should().BePositive();
        }

        [Fact]
        public async Task Login_WithCorrectCredentials_Returns200AndValidToken()
        {
            // Arrange - organiser@raceday.co.za was seeded with TestPass123!
            var controller = new AuthController(_context, _passwordHasher, _tokenService);
            TestControllerHelper.SetAnonymous(controller);

            var dto = new LoginDto
            {
                Email = "organiser@raceday.co.za",
                Password = "TestPass123!"
            };

            // Act
            var result = await controller.Login(dto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var ok = (OkObjectResult)result;
            var response = ok.Value as AuthResponseDto;
            response.Should().NotBeNull();
            response!.Role.Should().Be("Organiser");
            response.Token.Should().NotBeNullOrWhiteSpace();
            response.OrganiserId.Should().Be(1);
        }

        [Fact]
        public async Task Login_WithIncorrectPassword_Returns401Unauthorized()
        {
            // Arrange
            var controller = new AuthController(_context, _passwordHasher, _tokenService);
            TestControllerHelper.SetAnonymous(controller);

            var dto = new LoginDto
            {
                Email = "organiser@raceday.co.za",
                Password = "WrongPassword999!"
            };

            // Act
            var result = await controller.Login(dto);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
            var unauth = (UnauthorizedObjectResult)result;
            unauth.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task Login_WithNonExistentEmail_Returns401Unauthorized()
        {
            // Arrange
            var controller = new AuthController(_context, _passwordHasher, _tokenService);
            TestControllerHelper.SetAnonymous(controller);

            var dto = new LoginDto
            {
                Email = "nobody@raceday.co.za",
                Password = "TestPass123!"
            };

            // Act
            var result = await controller.Login(dto);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public void PasswordHasher_HashesSecurelyAndVerifiesAccurately()
        {
            // Act
            string hash = _passwordHasher.HashPassword("PlainSecret123!");

            // Assert - password must NOT be stored in plain text
            hash.Should().NotBe("PlainSecret123!");
            hash.Should().StartWith("$2"); // BCrypt signature

            _passwordHasher.VerifyPassword("PlainSecret123!", hash).Should().BeTrue();
            _passwordHasher.VerifyPassword("WrongSecret!", hash).Should().BeFalse();
        }
    }
}
