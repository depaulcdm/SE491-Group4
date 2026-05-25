using BEEST;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<LexiconOptions>(
    builder.Configuration.GetSection(LexiconOptions.SectionName));

builder.Services.AddSingleton<LexiconRegistry>();
builder.Services.AddSingleton<PhoneInventory>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://localhost:3000",
                "http://localhost:5173",
                "https://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");

app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithOpenApi();

app.MapGet("/api/cmudict/search", (
        [FromServices] LexiconRegistry lexicons,
        [FromQuery] string q,
        [FromQuery] string mode = "prefix",
        [FromQuery] int limit = 50,
        [FromQuery] string lang = "en") =>
    {
        if (string.IsNullOrWhiteSpace(q))
            return Results.BadRequest(new { error = "Query parameter 'q' is required." });

        var normalizedLang = string.IsNullOrWhiteSpace(lang) ? "en" : lang.Trim();

        if (!lexicons.TryGetLexicon(normalizedLang, out var lexicon) || lexicon is null)
            return Results.BadRequest(new { error = "Invalid lang; use 'en' or 'es'." });

        if (!Enum.TryParse<SearchMode>(mode, ignoreCase: true, out var searchMode))
            return Results.BadRequest(new { error = "Invalid mode; use 'prefix' or 'contains'." });

        var hits = lexicon.Search(q, searchMode, limit);
        var dto = new CmudictSearchResponseDto(
            q.Trim(),
            searchMode.ToString().ToLowerInvariant(),
            hits.Select(h => new CmudictSearchHitDto(h.Word, h.Pronunciations)).ToList());

        return Results.Ok(dto);
    })
    .WithName("CmudictSearch")
    .WithOpenApi();

app.MapPost("/api/worksheet/generate", (
        [FromServices] LexiconRegistry lexicons,
        [FromServices] PhoneInventory phones,
        [FromBody] WorksheetFilterCriteriaDto? criteria) =>
    {
        if (criteria is null)
            return Results.BadRequest(new { error = "Request body is required." });

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(criteria, null, null);
        if (!Validator.TryValidateObject(criteria, validationContext, validationResults, validateAllProperties: true))
        {
            var errors = validationResults
                .SelectMany(result => result.MemberNames.DefaultIfEmpty(string.Empty), (result, member) => new
                {
                    Member = member,
                    result.ErrorMessage,
                })
                .GroupBy(entry => entry.Member)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(entry => entry.ErrorMessage ?? "Invalid value.").ToArray());

            return Results.BadRequest(new { errors });
        }

        var normalizedLang = criteria.Language.Trim().ToLowerInvariant();
        if (!lexicons.TryGetLexicon(normalizedLang, out var lexicon) || lexicon is null)
            return Results.BadRequest(new { error = "Invalid lang; use 'en' or 'es'." });

        var response = lexicon.GenerateWorksheet(criteria, phones);
        return Results.Ok(response);
    })
    .WithName("GenerateWorksheet")
    .WithOpenApi();

app.Run();

public partial class Program { }
