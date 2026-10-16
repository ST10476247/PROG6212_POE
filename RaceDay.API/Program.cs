using System;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RaceDay.API.Data;
using RaceDay.API.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Configuration (EF Core Code-First with SQL Server & fallback)
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=(localdb)\\mssqllocaldb;Database=RaceDayDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

builder.Services.AddDbContext<RaceDayDbContext>(options =>
{
    // If running in test or if SQL server is not available, allow in-memory
    string? useInMemory = Environment.GetEnvironmentVariable("USE_INMEMORY_DB");
    if (useInMemory == "true" || builder.Environment.IsEnvironment("Testing"))
    {
        options.UseInMemoryDatabase("RaceDayTestDb");
    }
    else
    {
        try
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
            });
        }
        catch
        {
            options.UseInMemoryDatabase("RaceDayFallbackDb");
        }
    }
});

// 2. Session Management (Maintains user authentication state and role)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(120);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".RaceDay.Session";
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// 3. Authentication & JWT Bearer Token Support
string jwtSecret = builder.Configuration["Jwt:SecretKey"] ?? "RaceDaySouthAfricaSecretKeyLongEnoughForHmacSha256Security2026!";
string jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "RaceDayAPI";
string jwtAudience = builder.Configuration["Jwt:Audience"] ?? "RaceDayClients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 4. Register Application Services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();

// 5. Register Controllers with JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// 6. Enable CORS for local web clients / Swagger
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 7. Swagger / OpenAPI Integration with Rich Descriptions and Auth
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RaceDay RESTful API",
        Version = "v1",
        Description = "### South African Road Event Management Platform (PROG6212 PoE - Part 2)\n" +
                      "RESTful Web API for road running, cycling, and walking events (Comrades Marathon, Cape Town Cycle Tour, Soweto Marathon).\n\n" +
                      "**User Roles & Session Authentication:**\n" +
                      "- **Organiser**: Full event creation, category configuration, entrant roster inspection, timing mat results capture.\n" +
                      "- **Participant**: Race discovery, category enrolment with automated bib allocation, ticket history, live weather advice.\n\n" +
                      "*Session management and JWT Bearer tokens are both supported.*",
        Contact = new OpenApiContact
        {
            Name = "Lesedi Mogatwe (ST10476247)",
            Email = "st10476247@rcconnect.edu.za"
        }
    });

    c.EnableAnnotations();

    // Include XML Documentation Comments for rich endpoint descriptions
    string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // JWT Security Scheme in Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// 8. Auto-Seed Database on Startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<RaceDayDbContext>();
        DbInitializer.Initialize(context);
        Console.WriteLine("Database and South African seed data initialized successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database initialization notice: {ex.Message}");
    }
}

// 9. HTTP Request Pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RaceDay RESTful API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "RaceDay API - Swagger Documentation";
    c.DisplayRequestDuration();
});

app.UseCors("AllowAll");
app.UseRouting();

// Session must come before Authentication
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Default root redirects directly to Swagger UI
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();

// Partial Program class for WebApplicationFactory integration in tests
public partial class Program { }
