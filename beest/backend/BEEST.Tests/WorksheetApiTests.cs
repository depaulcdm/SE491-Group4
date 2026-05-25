using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BEEST.Tests;

public class WorksheetApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public WorksheetApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GenerateWorksheet_ReturnsResultsForValidRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/worksheet/generate", new
        {
            totalWordCount = 5,
            language = "en",
            randomSeed = 42,
            includedPhonemes = "AH",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(5, body.GetProperty("requestedWordCount").GetInt32());
        Assert.True(body.GetProperty("totalMatchesFound").GetInt32() > 0);
        Assert.True(body.GetProperty("returnedWordCount").GetInt32() <= 5);
        Assert.Equal(42, body.GetProperty("randomSeed").GetInt32());
        Assert.True(body.GetProperty("results").GetArrayLength() > 0);
    }

    [Fact]
    public async Task GenerateWorksheet_ReturnsBadRequestForInvalidLanguage()
    {
        var response = await _client.PostAsJsonAsync("/api/worksheet/generate", new
        {
            totalWordCount = 5,
            language = "fr",
            randomSeed = 1,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Invalid lang; use 'en' or 'es'.", body.GetProperty("error").GetString());
    }

    [Fact]
    public async Task GenerateWorksheet_ReturnsValidationErrorsForInvalidCriteria()
    {
        var response = await _client.PostAsJsonAsync("/api/worksheet/generate", new
        {
            totalWordCount = 0,
            language = "en",
            randomSeed = 1,
            includedSyllableStructure = "CV1",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("TotalWordCount", out _));
        Assert.True(errors.TryGetProperty("IncludedSyllableStructure", out _));
    }
}
