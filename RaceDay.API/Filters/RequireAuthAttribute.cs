using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RaceDay.API.Common;

namespace RaceDay.API.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireAuthAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.GetCurrentUser();

            if (!user.IsAuthenticated)
            {
                context.Result = new JsonResult(new
                {
                    status = 401,
                    message = "Unauthorized. You must be logged in with an active session or valid Bearer token to access this resource."
                })
                {
                    StatusCode = 401
                };
            }
        }
    }
}
