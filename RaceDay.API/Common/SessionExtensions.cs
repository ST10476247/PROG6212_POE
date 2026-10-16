using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace RaceDay.API.Common
{
    public class CurrentUserContext
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? OrganiserId { get; set; }
        public int? ParticipantId { get; set; }
        public bool IsAuthenticated => UserId > 0 && !string.IsNullOrEmpty(Role);
    }

    public static class SessionExtensions
    {
        public const string SessionUserId = "Auth_UserId";
        public const string SessionEmail = "Auth_Email";
        public const string SessionFullName = "Auth_FullName";
        public const string SessionRole = "Auth_Role";
        public const string SessionOrganiserId = "Auth_OrganiserId";
        public const string SessionParticipantId = "Auth_ParticipantId";

        public static void SetUserSession(this ISession session, int userId, string email, string fullName, string role, int? organiserId, int? participantId)
        {
            session.SetInt32(SessionUserId, userId);
            session.SetString(SessionEmail, email);
            session.SetString(SessionFullName, fullName);
            session.SetString(SessionRole, role);

            if (organiserId.HasValue)
                session.SetInt32(SessionOrganiserId, organiserId.Value);
            else
                session.Remove(SessionOrganiserId);

            if (participantId.HasValue)
                session.SetInt32(SessionParticipantId, participantId.Value);
            else
                session.Remove(SessionParticipantId);
        }

        public static void ClearUserSession(this ISession session)
        {
            session.Clear();
        }

        public static CurrentUserContext GetCurrentUser(this HttpContext httpContext)
        {
            var ctx = new CurrentUserContext();

            // 1. Check Session first
            if (httpContext.Session != null)
            {
                int? sUserId = httpContext.Session.GetInt32(SessionUserId);
                if (sUserId.HasValue && sUserId.Value > 0)
                {
                    ctx.UserId = sUserId.Value;
                    ctx.Email = httpContext.Session.GetString(SessionEmail) ?? string.Empty;
                    ctx.FullName = httpContext.Session.GetString(SessionFullName) ?? string.Empty;
                    ctx.Role = httpContext.Session.GetString(SessionRole) ?? string.Empty;
                    ctx.OrganiserId = httpContext.Session.GetInt32(SessionOrganiserId);
                    ctx.ParticipantId = httpContext.Session.GetInt32(SessionParticipantId);
                    return ctx;
                }
            }

            // 2. Check JWT Claims
            if (httpContext.User?.Identity?.IsAuthenticated == true)
            {
                var idClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(idClaim, out int uid))
                {
                    ctx.UserId = uid;
                    ctx.Email = httpContext.User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
                    ctx.FullName = httpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
                    ctx.Role = httpContext.User.FindFirst(ClaimTypes.Role)?.Value 
                               ?? httpContext.User.FindFirst("Role")?.Value 
                               ?? string.Empty;

                    var orgClaim = httpContext.User.FindFirst("OrganiserId")?.Value;
                    if (int.TryParse(orgClaim, out int orgId)) ctx.OrganiserId = orgId;

                    var partClaim = httpContext.User.FindFirst("ParticipantId")?.Value;
                    if (int.TryParse(partClaim, out int partId)) ctx.ParticipantId = partId;

                    return ctx;
                }
            }

            return ctx;
        }
    }
}
