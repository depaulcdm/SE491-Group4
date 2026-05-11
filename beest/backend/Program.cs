using BEEST;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<LexiconOptions>(
    builder.Configuration.GetSection(LexiconOptions.SectionName));

builder.Services.AddSingleton<LexiconRegistry>();

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

app.Run();
