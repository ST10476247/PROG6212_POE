using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RaceDay.API.Models.Entities;

namespace RaceDay.API.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user, int? organiserId, int? participantId, out DateTime expiresAt);
    }

    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user, int? organiserId, int? participantId, out DateTime expiresAt)
        {
            string secret = _configuration["Jwt:SecretKey"] ?? "RaceDaySouthAfricaSecretKeyLongEnoughForHmacSha256Security2026!";
            string issuer = _configuration["Jwt:Issuer"] ?? "RaceDayAPI";
            string audience = _configuration["Jwt:Audience"] ?? "RaceDayClients";
            int expiryMinutes = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out int exp) ? exp : 120;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("Role", user.Role)
            };

            if (organiserId.HasValue)
            {
                claims.Add(new Claim("OrganiserId", organiserId.Value.ToString()));
            }

            if (participantId.HasValue)
            {
                claims.Add(new Claim("ParticipantId", participantId.Value.ToString()));
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
