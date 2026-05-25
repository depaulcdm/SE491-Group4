using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using BEEST;

namespace BEEST.Tests;

public class FunctionalSearchTests : IDisposable
{
    private readonly string _englishDictPath;
    private readonly string _spanishDictPath;
    private readonly CmudictLexicon _englishLexicon;
    private readonly CmudictLexicon _spanishLexicon;
    private readonly PhoneInventory _phones;

    public FunctionalSearchTests()
    {
        _englishDictPath = Path.GetTempFileName();
        File.WriteAllLines(_englishDictPath, new[]
        {
            "hello HH AH L OW",
            "hello(2) HH EH L OW",
            "world W ER L D",
            "test T EH S T",
            "testing T EH S T IH NG",
            "cat K AE T"
        });

        _spanishDictPath = Path.GetTempFileName();
        File.WriteAllLines(_spanishDictPath, new[]
        {
            "hola O L A",
            "mundo M U N D O",
            "gato G A T O"
        });

        var logger = NullLogger<CmudictLexicon>.Instance;

        _englishLexicon = new CmudictLexicon(null!, _englishDictPath, logger);
        _spanishLexicon = new CmudictLexicon(null!, _spanishDictPath, logger);
        _phones = TestPhoneInventory.CreateDefault();
    }

    public void Dispose()
    {
        if (File.Exists(_englishDictPath)) File.Delete(_englishDictPath);
        if (File.Exists(_spanishDictPath)) File.Delete(_spanishDictPath);
    }

    [Fact]
    public void SuccessfulTargetWordLookup()
    {
        var criteria = new WorksheetFilterCriteriaDto(
            TotalWordCount: 50,
            Language: "en",
            RandomSeed: 1,
            IncludedPhonemes: "HH");
        var response = _englishLexicon.GenerateWorksheet(criteria, _phones);

        Assert.Equal(1, response.TotalMatchesFound);
        Assert.Single(response.Results);
        Assert.Equal("hello", response.Results[0].Word);
        Assert.Equal("HH AH L OW", response.Results[0].Pronunciation);
    }

    [Fact]
    public void TextNormalization_CaseInsensitive()
    {
        var criteria = new WorksheetFilterCriteriaDto(
            TotalWordCount: 50,
            Language: "en",
            RandomSeed: 1,
            IncludedPhonemes: "hh");
        var response = _englishLexicon.GenerateWorksheet(criteria, _phones);

        Assert.Single(response.Results);
        Assert.Equal("hello", response.Results[0].Word);
    }

    [Fact]
    public void LanguageBoundaryIsolation()
    {
        var englishCriteria = new WorksheetFilterCriteriaDto(
            TotalWordCount: 50,
            Language: "en",
            RandomSeed: 1,
            IncludedPhonemes: "U");
        var englishResponse = _englishLexicon.GenerateWorksheet(englishCriteria, _phones);

        Assert.Empty(englishResponse.Results);

        var spanishCriteria = new WorksheetFilterCriteriaDto(
            TotalWordCount: 50,
            Language: "es",
            RandomSeed: 1,
            IncludedPhonemes: "L");
        var spanishResponse = _spanishLexicon.GenerateWorksheet(spanishCriteria, _phones);

        Assert.Single(spanishResponse.Results);
        Assert.Equal("hola", spanishResponse.Results[0].Word);
    }

    [Fact]
    public void GracefulHandlingOfZeroMatches()
    {
        var criteria = new WorksheetFilterCriteriaDto(
            TotalWordCount: 50,
            Language: "en",
            RandomSeed: 1,
            IncludedPhonemes: "ZZ");
        var response = _englishLexicon.GenerateWorksheet(criteria, _phones);

        Assert.Empty(response.Results);
        Assert.Equal(0, response.TotalMatchesFound);
    }

    [Fact]
    public void BasicFiltering_IncludedPhonemes()
    {
        var criteria = new WorksheetFilterCriteriaDto(
            TotalWordCount: 50,
            Language: "en",
            RandomSeed: 1,
            IncludedPhonemes: "IH");
        var response = _englishLexicon.GenerateWorksheet(criteria, _phones);

        Assert.Single(response.Results);
        Assert.Equal("testing", response.Results[0].Word);
    }

    [Fact]
    public void BasicFiltering_ExcludedPhonemes()
    {
        var excludedByAlternatePronunciation = new WorksheetFilterCriteriaDto(
            TotalWordCount: 50,
            Language: "en",
            RandomSeed: 1,
            IncludedPhonemes: "HH",
            ExcludedPhonemes: "EH");
        var excludedResponse = _englishLexicon.GenerateWorksheet(excludedByAlternatePronunciation, _phones);

        Assert.Empty(excludedResponse.Results);

        var excludedFromAllPronunciations = new WorksheetFilterCriteriaDto(
            TotalWordCount: 50,
            Language: "en",
            RandomSeed: 1,
            IncludedPhonemes: "HH",
            ExcludedPhonemes: "OW");
        var emptyResponse = _englishLexicon.GenerateWorksheet(excludedFromAllPronunciations, _phones);

        Assert.Empty(emptyResponse.Results);

        var allowedCriteria = new WorksheetFilterCriteriaDto(
            TotalWordCount: 50,
            Language: "en",
            RandomSeed: 1,
            IncludedPhonemes: "K",
            ExcludedPhonemes: "NG");
        var allowedResponse = _englishLexicon.GenerateWorksheet(allowedCriteria, _phones);

        Assert.Contains(allowedResponse.Results, result => result.Word == "cat");
    }

    [Fact]
    public void TotalWordCount_LimitIsRespected()
    {
        var criteria = new WorksheetFilterCriteriaDto(
            TotalWordCount: 1,
            Language: "en",
            RandomSeed: 1,
            IncludedPhonemes: "EH");
        var response = _englishLexicon.GenerateWorksheet(criteria, _phones);

        Assert.Equal(1, response.ReturnedWordCount);
        Assert.Single(response.Results);
        Assert.True(response.TotalMatchesFound > 1);
    }
}
