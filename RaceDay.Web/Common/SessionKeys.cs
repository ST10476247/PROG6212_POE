namespace RaceDay.Web.Common
{
    /// <summary>
    /// Central definition of the session keys used to persist the authenticated
    /// user's identity, role and API token between requests.
    /// </summary>
    public static class SessionKeys
    {
        public const string UserId = "Auth_UserId";
        public const string Email = "Auth_Email";
        public const string FullName = "Auth_FullName";
        public const string Role = "Auth_Role";
        public const string OrganiserId = "Auth_OrganiserId";
        public const string ParticipantId = "Auth_ParticipantId";
        public const string ProfilePictureUrl = "Auth_ProfilePictureUrl";
        public const string Token = "Auth_Token";
    }

    /// <summary>
    /// Lightweight representation of the currently authenticated user, rebuilt
    /// from the session on every request.
    /// </summary>
    public class SessionUser
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? OrganiserId { get; set; }
        public int? ParticipantId { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string Token { get; set; } = string.Empty;

        public bool IsAuthenticated => UserId > 0 && !string.IsNullOrEmpty(Role);
        public bool IsOrganiser => string.Equals(Role, "Organiser", StringComparison.OrdinalIgnoreCase);
        public bool IsParticipant => string.Equals(Role, "Participant", StringComparison.OrdinalIgnoreCase);
    }
}
