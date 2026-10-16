using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RaceDay.API.Common;

namespace RaceDay.API.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _allowedRoles;

        public RequireRoleAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.GetCurrentUser();

            // 1. Check if user is authenticated at all
            if (!user.IsAuthenticated)
            {
                context.Result = new JsonResult(new
                {
                    status = 401,
                    message = "Unauthorized. Please authenticate with valid credentials first."
                })
                {
                    StatusCode = 401
                };
                return;
            }

            // 2. Check if user has required role
            if (_allowedRoles.Length > 0 && !_allowedRoles.Contains(user.Role, StringComparer.OrdinalIgnoreCase))
            {
                context.Result = new JsonResult(new
                {
                    status = 403,
                    message = $"Forbidden. Access denied for role '{user.Role}'. This endpoint strictly requires one of: [{string.Join(", ", _allowedRoles)}]."
                })
                {
                    StatusCode = 403
                };
            }
        }
    }
}
