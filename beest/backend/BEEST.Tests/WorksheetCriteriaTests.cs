using System.ComponentModel.DataAnnotations;
using Xunit;
using BEEST;

namespace BEEST.Tests;

public class WorksheetCriteriaTests
{
    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);
        return validationResults;
    }

    [Fact]
    public void ValidCriteria_PassesValidation()
    {
        var criteria = new WorksheetFilterCriteriaDto(50, "en", RandomSeed: 42);
        var results = ValidateModel(criteria);
        Assert.Empty(results);
    }

    [Fact]
    public void ValidCriteria_WithOptionalFilters_PassesValidation()
    {
        var criteria = new WorksheetFilterCriteriaDto(
            TotalWordCount: 25,
            Language: "es",
            RandomSeed: 7,
            IncludedPhonemes: "K AE",
            ExcludedPhonemes: "OW",
            SyllableCount: 2,
            IncludedSyllableStructure: "CVCV");

        var results = ValidateModel(criteria);
        Assert.Empty(results);
    }

    [Fact]
    public void ExactlyOneWord_PassesValidation()
    {
        var criteria = new WorksheetFilterCriteriaDto(1, "es", RandomSeed: 1);
        var results = ValidateModel(criteria);
        Assert.Empty(results);
    }

    [Fact]
    public void Exactly100Words_PassesValidation()
    {
        var criteria = new WorksheetFilterCriteriaDto(100, "en", RandomSeed: 99);
        var results = ValidateModel(criteria);
        Assert.Empty(results);
    }

    [Fact]
    public void ZeroWords_FailsValidation()
    {
        var criteria = new WorksheetFilterCriteriaDto(0, "en", RandomSeed: 1);
        var results = ValidateModel(criteria);
        Assert.Contains(results, r => r.MemberNames.Contains("TotalWordCount"));
    }

    [Fact]
    public void NegativeWords_FailsValidation()
    {
        var criteria = new WorksheetFilterCriteriaDto(-5, "en", RandomSeed: 1);
        var results = ValidateModel(criteria);
        Assert.Contains(results, r => r.MemberNames.Contains("TotalWordCount"));
    }

    [Fact]
    public void Over100Words_FailsValidation()
    {
        var criteria = new WorksheetFilterCriteriaDto(101, "en", RandomSeed: 1);
        var results = ValidateModel(criteria);
        Assert.Contains(results, r => r.MemberNames.Contains("TotalWordCount"));
    }

    [Fact]
    public void MissingLanguage_FailsValidation()
    {
        var criteria = new WorksheetFilterCriteriaDto(50, null!, RandomSeed: 1);
        var results = ValidateModel(criteria);
        Assert.Contains(results, r => r.MemberNames.Contains("Language"));
    }

    [Fact]
    public void ZeroSyllableCount_FailsValidation()
    {
        var criteria = new WorksheetFilterCriteriaDto(50, "en", RandomSeed: 1, SyllableCount: 0);
        var results = ValidateModel(criteria);
        Assert.Contains(results, r => r.MemberNames.Contains("SyllableCount"));
    }

    [Fact]
    public void OverMaxSyllableCount_FailsValidation()
    {
        var criteria = new WorksheetFilterCriteriaDto(50, "en", RandomSeed: 1, SyllableCount: 21);
        var results = ValidateModel(criteria);
        Assert.Contains(results, r => r.MemberNames.Contains("SyllableCount"));
    }

    [Fact]
    public void InvalidSyllableStructure_FailsValidation()
    {
        var criteria = new WorksheetFilterCriteriaDto(
            50,
            "en",
            RandomSeed: 1,
            IncludedSyllableStructure: "CV1");

        var results = ValidateModel(criteria);
        Assert.Contains(results, r => r.MemberNames.Contains("IncludedSyllableStructure"));
    }

    [Fact]
    public void ValidSyllableStructure_IsCaseInsensitivePattern()
    {
        var criteria = new WorksheetFilterCriteriaDto(
            50,
            "en",
            RandomSeed: 1,
            IncludedSyllableStructure: "cVcV");

        var results = ValidateModel(criteria);
        Assert.Empty(results);
    }
}
