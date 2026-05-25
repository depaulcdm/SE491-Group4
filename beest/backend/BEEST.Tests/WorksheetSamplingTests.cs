using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using BEEST;

namespace BEEST.Tests;

public class WorksheetSamplingTests : IDisposable
{
    private readonly string _englishDictPath;
    private readonly CmudictLexicon _lexicon;
    private readonly PhoneInventory _phones;

    public WorksheetSamplingTests()
    {
        _englishDictPath = Path.GetTempFileName();
        File.WriteAllLines(_englishDictPath, Enumerable.Range(1, 10).Select(i => $"word{i} T EH1 S T {i}"));
        _lexicon = new CmudictLexicon(null!, _englishDictPath, NullLogger<CmudictLexicon>.Instance);
        _phones = TestPhoneInventory.CreateDefault();
    }

    public void Dispose()
    {
        if (File.Exists(_englishDictPath))
            File.Delete(_englishDictPath);
    }

    [Fact]
    public void SeededSample_ReturnsRequestedCountWhenPoolIsLarger()
    {
        var criteria = new WorksheetFilterCriteriaDto(
            TotalWordCount: 5,
            Language: "en",
            RandomSeed: 42,
            IncludedPhonemes: "EH");

        var response = _lexicon.GenerateWorksheet(criteria, _phones);

        Assert.Equal(10, response.TotalMatchesFound);
        Assert.Equal(5, response.ReturnedWordCount);
        Assert.Equal(5, response.Results.Count);
    }

    [Fact]
    public void SeededSample_IsRepeatableForSameSeed()
    {
        var criteria = new WorksheetFilterCriteriaDto(
            TotalWordCount: 5,
            Language: "en",
            RandomSeed: 42,
            IncludedPhonemes: "EH");

        var first = _lexicon.GenerateWorksheet(criteria, _phones);
        var second = _lexicon.GenerateWorksheet(criteria, _phones);

        Assert.Equal(
            first.Results.Select(result => result.Word).ToArray(),
            second.Results.Select(result => result.Word).ToArray());
    }

    [Fact]
    public void SeededSample_ReturnsAllMatchesWhenPoolIsSmallerThanRequested()
    {
        var criteria = new WorksheetFilterCriteriaDto(
            TotalWordCount: 20,
            Language: "en",
            RandomSeed: 7,
            IncludedPhonemes: "EH");

        var response = _lexicon.GenerateWorksheet(criteria, _phones);

        Assert.Equal(10, response.TotalMatchesFound);
        Assert.Equal(10, response.ReturnedWordCount);
        Assert.Equal(10, response.Results.Count);
        Assert.Equal(20, response.RequestedWordCount);
    }
}
