using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RaceDay.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// MVC + Razor view engine
// ---------------------------------------------------------------------------
builder.Services.AddControllersWithViews();

// Provides access to HttpContext (and therefore the session) inside services
builder.Services.AddHttpContextAccessor();

// ---------------------------------------------------------------------------
// Session management — holds the authenticated user's identity, role and JWT
// token so the MVC app can stay logged in across all pages.
// ---------------------------------------------------------------------------
int sessionMinutes = builder.Configuration.GetValue<int?>("Session:TimeoutMinutes") ?? 60;

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(sessionMinutes);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".RaceDay.Web.Session";
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// ---------------------------------------------------------------------------
// Typed HttpClient that talks to the RaceDay Part 2 REST API.
// The MVC application NEVER touches the database or Azure directly — every
// data operation is delegated to the API through IRaceDayApiClient.
// ---------------------------------------------------------------------------
string apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5111";

builder.Services.AddHttpClient<IRaceDayApiClient, RaceDayApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(45);
});

var app = builder.Build();

// ---------------------------------------------------------------------------
// HTTP request pipeline
// ---------------------------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

// Session must be established before controllers read the logged-in user.
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Lightweight health probe used by docker-compose healthcheck.
app.MapGet("/health", () => Results.Ok("Healthy"));

app.Run();

// Exposed for potential integration testing / CI.
public partial class Program { }
