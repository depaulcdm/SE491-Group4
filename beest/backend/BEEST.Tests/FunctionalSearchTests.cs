using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Xunit;
using BEEST;
using System.IO;
using System.Linq;
using System;

namespace BEEST.Tests;

public class FunctionalSearchTests : IDisposable
{
    private readonly string _englishDictPath;
    private readonly string _spanishDictPath;
    private readonly CmudictLexicon _englishLexicon;
    private readonly CmudictLexicon _spanishLexicon;

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
    }

    public void Dispose()
    {
        if (File.Exists(_englishDictPath)) File.Delete(_englishDictPath);
        if (File.Exists(_spanishDictPath)) File.Delete(_spanishDictPath);
    }

    [Fact]
    public void SuccessfulTargetWordLookup()
    {
        var criteria = new WorksheetFilterCriteriaDto(TotalWordCount: 50, Language: "en", SearchTerm: "hello");
        var results = _englishLexicon.GenerateWorksheet(criteria);

        Assert.Single(results);
        Assert.Equal("hello", results[0].Word);
        Assert.Equal(2, results[0].Pronunciations.Count);
    }

    [Fact]
    public void TextNormalization_CaseInsensitive()
    {
        var criteria = new WorksheetFilterCriteriaDto(TotalWordCount: 50, Language: "en", SearchTerm: "hElLo");
        var results = _englishLexicon.GenerateWorksheet(criteria);

        Assert.Single(results);
        Assert.Equal("hello", results[0].Word);
    }

    [Fact]
    public void LanguageBoundaryIsolation()
    {
        var englishCriteria = new WorksheetFilterCriteriaDto(TotalWordCount: 50, Language: "en", SearchTerm: "hola");
        var englishResults = _englishLexicon.GenerateWorksheet(englishCriteria);

        Assert.Empty(englishResults);

        var spanishCriteria = new WorksheetFilterCriteriaDto(TotalWordCount: 50, Language: "es", SearchTerm: "hola");
        var spanishResults = _spanishLexicon.GenerateWorksheet(spanishCriteria);

        Assert.Single(spanishResults);
        Assert.Equal("hola", spanishResults[0].Word);
    }

    [Fact]
    public void GracefulHandlingOfZeroMatches()
    {
        var criteria = new WorksheetFilterCriteriaDto(TotalWordCount: 50, Language: "en", SearchTerm: "nonexistent");
        var results = _englishLexicon.GenerateWorksheet(criteria);

        Assert.Empty(results);
    }

    [Fact]
    public void BasicFiltering_IncludedPhonemes()
    {
        var criteria = new WorksheetFilterCriteriaDto(TotalWordCount: 50, Language: "en", SearchTerm: "test", IncludedPhonemes: "IH");
        var results = _englishLexicon.GenerateWorksheet(criteria);

        Assert.Single(results);
        Assert.Equal("testing", results[0].Word);
    }

    [Fact]
    public void BasicFiltering_ExcludedPhonemes()
    {
        var criteria = new WorksheetFilterCriteriaDto(TotalWordCount: 50, Language: "en", SearchTerm: "hello", ExcludedPhonemes: "EH");
        var results = _englishLexicon.GenerateWorksheet(criteria);

        Assert.Single(results);
        Assert.Equal("hello", results[0].Word);

        var criteria2 = new WorksheetFilterCriteriaDto(TotalWordCount: 50, Language: "en", SearchTerm: "hello", ExcludedPhonemes: "OW");
        var results2 = _englishLexicon.GenerateWorksheet(criteria2);

        Assert.Empty(results2);
    }

    [Fact]
    public void TotalWordCount_LimitIsRespected()
    {
        var criteria = new WorksheetFilterCriteriaDto(TotalWordCount: 1, Language: "en", SearchTerm: "test");
        var results = _englishLexicon.GenerateWorksheet(criteria);

        Assert.Single(results);
    }
}
