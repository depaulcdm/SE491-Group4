using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using BEEST;

namespace BEEST.Tests;

public class WorksheetMultiPronunciationTests : IDisposable
{
    private readonly string _englishDictPath;
    private readonly CmudictLexicon _lexicon;
    private readonly PhoneInventory _phones;

    public WorksheetMultiPronunciationTests()
    {
        _englishDictPath = Path.GetTempFileName();
        File.WriteAllLines(_englishDictPath, new[]
        {
            "hello HH AH0 L OW1",
            "hello(2) HH EH0 L OW1",
            "read R IY1 D",
            "read(2) R EH1 D",
            "test T EH1 S T",
        });

        _lexicon = new CmudictLexicon(null!, _englishDictPath, NullLogger<CmudictLexicon>.Instance);
        _phones = TestPhoneInventory.CreateDefault();
    }

    public void Dispose()
    {
        if (File.Exists(_englishDictPath))
            File.Delete(_englishDictPath);
    }

    [Fact]
    public void UsesFirstMatchingPronunciationOnly()
    {
        var criteria = new WorksheetFilterCriteriaDto(
            TotalWordCount: 10,
            Language: "en",
            RandomSeed: 1,
            IncludedPhonemes: "AH IY");

        var response = _lexicon.GenerateWorksheet(criteria, _phones);

        Assert.Equal(2, response.TotalMatchesFound);
        Assert.Equal("HH AH0 L OW1", response.Results.Single(result => result.Word == "hello").Pronunciation);
        Assert.Equal("R IY1 D", response.Results.Single(result => result.Word == "read").Pronunciation);
    }

    [Fact]
    public void ExcludesWordWhenAnyPronunciationContainsExcludedSound()
    {
        var criteria = new WorksheetFilterCriteriaDto(
            TotalWordCount: 10,
            Language: "en",
            RandomSeed: 1,
            IncludedPhonemes: "HH",
            ExcludedPhonemes: "EH");

        var response = _lexicon.GenerateWorksheet(criteria, _phones);

        Assert.DoesNotContain(response.Results, result => result.Word == "hello");
    }

    [Fact]
    public void SyllableStructureFilter_UsesWholeWordPattern()
    {
        var criteria = new WorksheetFilterCriteriaDto(
            TotalWordCount: 10,
            Language: "en",
            RandomSeed: 1,
            IncludedSyllableStructure: "CVCC");

        var response = _lexicon.GenerateWorksheet(criteria, _phones);

        Assert.Single(response.Results);
        Assert.Equal("test", response.Results[0].Word);
        Assert.Equal("T EH1 S T", response.Results[0].Pronunciation);
    }
}
