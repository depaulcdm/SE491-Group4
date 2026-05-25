using BEEST;
using Xunit;

namespace BEEST.Tests;

public class PronunciationAnalyzerTests : IDisposable
{
    private readonly string _englishPhonesPath;
    private readonly string _spanishPhonesPath;
    private readonly PhoneInventory _phones;

    public PronunciationAnalyzerTests()
    {
        _englishPhonesPath = Path.GetTempFileName();
        File.WriteAllLines(_englishPhonesPath, new[]
        {
            "AA\tvowel",
            "AE\tvowel",
            "AH\tvowel",
            "EH\tvowel",
            "OW\tvowel",
            "HH\taspirate",
            "L\tliquid",
            "K\tstop",
            "T\tstop",
            "W\tsemivowel",
            "CH\taffricate",
        });

        _spanishPhonesPath = Path.GetTempFileName();
        File.WriteAllLines(_spanishPhonesPath, new[]
        {
            "A\tvowel",
            "E\tvowel",
            "O\tvowel",
            "K\tstop",
            "S\tfricative",
            "RR\tliquid",
        });

        _phones = new PhoneInventory(_englishPhonesPath, _spanishPhonesPath);
    }

    public void Dispose()
    {
        if (File.Exists(_englishPhonesPath))
            File.Delete(_englishPhonesPath);
        if (File.Exists(_spanishPhonesPath))
            File.Delete(_spanishPhonesPath);
    }

    [Fact]
    public void CountSyllables_ReturnsStressMarkedPhoneCount()
    {
        Assert.Equal(2, PronunciationAnalyzer.CountSyllables("HH AH0 L OW1"));
        Assert.Equal(1, PronunciationAnalyzer.CountSyllables("K AE1 T"));
        Assert.Equal(0, PronunciationAnalyzer.CountSyllables("HH L"));
    }

    [Fact]
    public void ToCvPattern_BuildsWholeWordPattern()
    {
        Assert.Equal("CVCV", PronunciationAnalyzer.ToCvPattern("en", "HH AH0 L OW1", _phones));
        Assert.Equal("CVC", PronunciationAnalyzer.ToCvPattern("en", "K AE1 T", _phones));
    }

    [Fact]
    public void ToCvPattern_TreatsSemivowelsAndAffricatesAsConsonants()
    {
        Assert.Equal("CV", PronunciationAnalyzer.ToCvPattern("en", "W OW1", _phones));
        Assert.Equal("CV", PronunciationAnalyzer.ToCvPattern("en", "CH OW1", _phones));
    }

    [Fact]
    public void ToCvPattern_SpanishUsesSpanishInventory()
    {
        Assert.Equal("CVCV", PronunciationAnalyzer.ToCvPattern("es", "K A1 S A0", _phones));
        Assert.Equal("CCV", PronunciationAnalyzer.ToCvPattern("es", "K S A0", _phones));
    }

    [Fact]
    public void MatchesSyllableCount_RequiresExactMatch()
    {
        Assert.True(PronunciationAnalyzer.MatchesSyllableCount("HH AH0 L OW1", 2));
        Assert.False(PronunciationAnalyzer.MatchesSyllableCount("HH AH0 L OW1", 1));
    }

    [Fact]
    public void MatchesCvStructure_IsCaseInsensitive()
    {
        Assert.True(PronunciationAnalyzer.MatchesCvStructure("en", "HH AH0 L OW1", "CVCV", _phones));
        Assert.True(PronunciationAnalyzer.MatchesCvStructure("en", "HH AH0 L OW1", "cvcv", _phones));
        Assert.False(PronunciationAnalyzer.MatchesCvStructure("en", "HH AH0 L OW1", "CVC", _phones));
    }

    [Fact]
    public void ContainsAnyPhoneme_UsesOrLogic()
    {
        var tokens = PronunciationAnalyzer.ParsePhonemeTokens("IH OW");

        Assert.True(PronunciationAnalyzer.ContainsAnyPhoneme("HH AH0 L OW1", tokens));
        Assert.True(PronunciationAnalyzer.ContainsAnyPhoneme("HH EH0 L OW", tokens));
        Assert.False(PronunciationAnalyzer.ContainsAnyPhoneme("HH AH0 L", tokens));
    }

    [Fact]
    public void ContainsAnyPhoneme_ReturnsTrueWhenNoTokensProvided()
    {
        Assert.True(PronunciationAnalyzer.ContainsAnyPhoneme("HH AH0 L OW", Array.Empty<string>()));
    }

    [Fact]
    public void ContainsExcludedPhoneme_DetectsAnyExcludedToken()
    {
        var tokens = PronunciationAnalyzer.ParsePhonemeTokens("EH OW");

        Assert.True(PronunciationAnalyzer.ContainsExcludedPhoneme("HH EH0 L OW", tokens));
        Assert.False(PronunciationAnalyzer.ContainsExcludedPhoneme("HH AH0 L", tokens));
    }

    [Fact]
    public void ParsePhonemeTokens_SplitsOnWhitespace()
    {
        var tokens = PronunciationAnalyzer.ParsePhonemeTokens("  K   AE  T  ");

        Assert.Equal(["K", "AE", "T"], tokens);
    }
}

public class PhoneInventoryTests : IDisposable
{
    private readonly string _englishPhonesPath;
    private readonly string _spanishPhonesPath;

    public PhoneInventoryTests()
    {
        _englishPhonesPath = Path.GetTempFileName();
        File.WriteAllLines(_englishPhonesPath, new[]
        {
            "AH\tvowel",
            "OW\tvowel",
            "HH\taspirate",
        });

        _spanishPhonesPath = Path.GetTempFileName();
        File.WriteAllLines(_spanishPhonesPath, new[]
        {
            "A\tvowel",
            "K\tstop",
        });
    }

    public void Dispose()
    {
        if (File.Exists(_englishPhonesPath))
            File.Delete(_englishPhonesPath);
        if (File.Exists(_spanishPhonesPath))
            File.Delete(_spanishPhonesPath);
    }

    [Fact]
    public void IsVowel_StripsStressDigitsBeforeLookup()
    {
        var inventory = new PhoneInventory(_englishPhonesPath, _spanishPhonesPath);

        Assert.True(inventory.IsVowel("en", "AH0"));
        Assert.True(inventory.IsVowel("en", "OW2"));
        Assert.False(inventory.IsVowel("en", "HH"));
    }

    [Fact]
    public void Classify_ReturnsConsonantForNonVowels()
    {
        var inventory = new PhoneInventory(_englishPhonesPath, _spanishPhonesPath);

        Assert.Equal('V', inventory.Classify("en", "AH1"));
        Assert.Equal('C', inventory.Classify("en", "HH"));
        Assert.Equal('V', inventory.Classify("es", "A0"));
        Assert.Equal('C', inventory.Classify("es", "K"));
    }

    [Fact]
    public void StripStressDigits_LeavesUnstressedPhonesUnchanged()
    {
        Assert.Equal("HH", PhoneInventory.StripStressDigits("HH"));
        Assert.Equal("RR", PhoneInventory.StripStressDigits("RR"));
        Assert.Equal("AH", PhoneInventory.StripStressDigits("AH0"));
    }
}
