using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using RaceDay.API.Common;

namespace RaceDay.Tests.Helpers
{
    public class MockSession : ISession
    {
        private readonly Dictionary<string, byte[]> _storage = new();

        public bool IsAvailable => true;
        public string Id => Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _storage.Keys;

        public void Clear() => _storage.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key) => _storage.Remove(key);

        public void Set(string key, byte[] value) => _storage[key] = value;

        public bool TryGetValue(string key, out byte[] value) => _storage.TryGetValue(key, out value!);
    }

    public static class TestControllerHelper
    {
        public static DefaultHttpContext SetupHttpContext(int? userId = null, string? role = null, string? email = null, int? organiserId = null, int? participantId = null)
        {
            var httpContext = new DefaultHttpContext();
            var session = new MockSession();
            httpContext.Session = session;

            if (userId.HasValue && !string.IsNullOrEmpty(role))
            {
                session.SetUserSession(userId.Value, email ?? "test@raceday.co.za", "Test User", role, organiserId, participantId);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()),
                    new Claim(ClaimTypes.Role, role),
                    new Claim(ClaimTypes.Email, email ?? "test@raceday.co.za"),
                    new Claim(ClaimTypes.Name, "Test User")
                };

                if (organiserId.HasValue) claims.Add(new Claim("OrganiserId", organiserId.Value.ToString()));
                if (participantId.HasValue) claims.Add(new Claim("ParticipantId", participantId.Value.ToString()));

                var identity = new ClaimsIdentity(claims, "TestAuth");
                httpContext.User = new ClaimsPrincipal(identity);
            }

            return httpContext;
        }

        public static void SetOrganiser(ControllerBase controller, int userId = 1, int organiserId = 1)
        {
            var ctx = SetupHttpContext(userId, "Organiser", "organiser@raceday.co.za", organiserId: organiserId);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = ctx
            };
        }

        public static void SetParticipant(ControllerBase controller, int userId = 2, int participantId = 1)
        {
            var ctx = SetupHttpContext(userId, "Participant", "runner@raceday.co.za", participantId: participantId);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = ctx
            };
        }

        public static void SetAnonymous(ControllerBase controller)
        {
            var ctx = SetupHttpContext();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = ctx
            };
        }

        public static AuthorizationFilterContext CreateAuthFilterContext(HttpContext httpContext)
        {
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
        }
    }
}
