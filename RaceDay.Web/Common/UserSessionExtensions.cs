using Microsoft.AspNetCore.Http;

namespace RaceDay.Web.Common
{
    /// <summary>
    /// Helpers for reading and writing the authenticated user to the MVC session.
    /// </summary>
    public static class UserSessionExtensions
    {
        public static void SetUserSession(
            this ISession session,
            int userId,
            string email,
            string fullName,
            string role,
            int? organiserId,
            int? participantId,
            string token,
            string? profilePictureUrl = null)
        {
            session.SetInt32(SessionKeys.UserId, userId);
            session.SetString(SessionKeys.Email, email ?? string.Empty);
            session.SetString(SessionKeys.FullName, fullName ?? string.Empty);
            session.SetString(SessionKeys.Role, role ?? string.Empty);
            session.SetString(SessionKeys.Token, token ?? string.Empty);

            if (!string.IsNullOrWhiteSpace(profilePictureUrl))
                session.SetString(SessionKeys.ProfilePictureUrl, profilePictureUrl!);
            else
                session.Remove(SessionKeys.ProfilePictureUrl);

            if (organiserId.HasValue)
                session.SetInt32(SessionKeys.OrganiserId, organiserId.Value);
            else
                session.Remove(SessionKeys.OrganiserId);

            if (participantId.HasValue)
                session.SetInt32(SessionKeys.ParticipantId, participantId.Value);
            else
                session.Remove(SessionKeys.ParticipantId);
        }

        public static void ClearUserSession(this ISession session) => session.Clear();

        public static SessionUser GetCurrentUser(this ISession session)
        {
            var user = new SessionUser();
            if (session == null) return user;

            int? userId = session.GetInt32(SessionKeys.UserId);
            if (!userId.HasValue || userId.Value <= 0) return user;

            user.UserId = userId.Value;
            user.Email = session.GetString(SessionKeys.Email) ?? string.Empty;
            user.FullName = session.GetString(SessionKeys.FullName) ?? string.Empty;
            user.Role = session.GetString(SessionKeys.Role) ?? string.Empty;
            user.OrganiserId = session.GetInt32(SessionKeys.OrganiserId);
            user.ParticipantId = session.GetInt32(SessionKeys.ParticipantId);
            user.ProfilePictureUrl = session.GetString(SessionKeys.ProfilePictureUrl);
            user.Token = session.GetString(SessionKeys.Token) ?? string.Empty;
            return user;
        }

        /// <summary>Updates the cached profile picture URL and full name in the session.</summary>
        public static void UpdateProfileCache(this ISession session, string? fullName, string? profilePictureUrl)
        {
            if (!string.IsNullOrWhiteSpace(fullName))
                session.SetString(SessionKeys.FullName, fullName);

            if (!string.IsNullOrWhiteSpace(profilePictureUrl))
                session.SetString(SessionKeys.ProfilePictureUrl, profilePictureUrl!);
        }

        public static SessionUser GetCurrentUser(this HttpContext context)
            => context.Session.GetCurrentUser();

        public static bool IsLoggedIn(this HttpContext context)
            => context.Session.GetCurrentUser().IsAuthenticated;
    }
}
