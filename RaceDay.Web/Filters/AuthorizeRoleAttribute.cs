using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RaceDay.Web.Common;

namespace RaceDay.Web.Filters
{
    /// <summary>
    /// Session-based authorisation guard for MVC controllers.
    ///
    /// Usage:
    ///   [AuthorizeRole]                       → any authenticated user
    ///   [AuthorizeRole("Organiser")]          → Organiser only
    ///   [AuthorizeRole("Participant")]        → Participant only
    ///
    /// Unauthenticated users are redirected to the login page (with a return
    /// URL). Authenticated users with the wrong role are sent to Access Denied.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _allowedRoles;

        public AuthorizeRoleAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.Session.GetCurrentUser();

            if (!user.IsAuthenticated)
            {
                string returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.Result = new RedirectToActionResult(
                    "Login", "Account", new { returnUrl });
                return;
            }

            if (_allowedRoles.Length > 0 &&
                !_allowedRoles.Contains(user.Role, StringComparer.OrdinalIgnoreCase))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }
        }
    }
}
