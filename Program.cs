using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RaceDay.Models;
using RaceDay.Services;

var builder = WebApplication.CreateBuilder(args);

// Register DataStore Singleton
builder.Services.AddSingleton<DataStore>();

var app = builder.Build();

// Get root directory and wwwroot directory
string contentRoot = app.Environment.ContentRootPath;
string wwwroot = Path.Combine(contentRoot, "wwwroot");
Directory.CreateDirectory(wwwroot);

// Generate ERD diagram files (PNG, PDF, SVG, Draw.io) at startup
try
{
    Console.WriteLine("Generating RaceDay ERD Diagrams (PNG, PDF, SVG, Draw.io)...");
    ErdGenerator.GenerateAll(contentRoot);
    ErdGenerator.GenerateAll(wwwroot);
    Console.WriteLine("ERD Diagrams successfully generated!");
}
catch (Exception ex)
{
    Console.WriteLine($"Warning generating ERD diagrams: {ex.Message}");
}

app.UseDefaultFiles();
app.UseStaticFiles();

// API Endpoints
var dataStore = app.Services.GetRequiredService<DataStore>();

app.MapGet("/api/events", () => Results.Ok(dataStore.Events));

app.MapPost("/api/events", (Event newEv) =>
{
    var created = dataStore.AddEvent(newEv);
    return Results.Created($"/api/events/{created.EventID}", created);
});

app.MapGet("/api/participants", () => Results.Ok(dataStore.Participants));

app.MapGet("/api/entries", () => Results.Ok(dataStore.Entries));

app.MapPost("/api/entries", (EntryRegistrationDto dto) =>
{
    var newEntry = dataStore.AddEntry(dto.ParticipantId, dto.CategoryId, dto.MedicalNotes ?? "None");
    return Results.Ok(newEntry);
});

app.MapGet("/api/results", () => Results.Ok(dataStore.Results));

app.MapGet("/api/erd/download/{format}", (string format) =>
{
    string fileName = format.ToLower() switch
    {
        "pdf" => "RaceDay_ERD.pdf",
        "png" => "RaceDay_ERD.png",
        "svg" => "RaceDay_ERD.svg",
        "drawio" => "RaceDay_ERD.drawio",
        _ => "RaceDay_ERD.png"
    };

    string filePath = Path.Combine(contentRoot, fileName);
    if (!File.Exists(filePath)) return Results.NotFound();

    string mimeType = format.ToLower() switch
    {
        "pdf" => "application/pdf",
        "png" => "image/png",
        "svg" => "image/svg+xml",
        "drawio" => "application/xml",
        _ => "application/octet-stream"
    };

    return Results.File(filePath, mimeType, fileName);
});

app.Run();

public record EntryRegistrationDto(int ParticipantId, int CategoryId, string? MedicalNotes);
