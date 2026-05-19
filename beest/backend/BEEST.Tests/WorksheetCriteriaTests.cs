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
        var criteria = new WorksheetFilterCriteriaDto(50, "en");
        var results = ValidateModel(criteria);
        Assert.Empty(results);
    }

    [Fact]
    public void ExactlyOneWord_PassesValidation()
    {
        var criteria = new WorksheetFilterCriteriaDto(1, "es");
        var results = ValidateModel(criteria);
        Assert.Empty(results);
    }

    [Fact]
    public void Exactly100Words_PassesValidation()
    {
        var criteria = new WorksheetFilterCriteriaDto(100, "en");
        var results = ValidateModel(criteria);
        Assert.Empty(results);
    }

    [Fact]
    public void ZeroWords_FailsValidation()
    {
        var criteria = new WorksheetFilterCriteriaDto(0, "en");
        var results = ValidateModel(criteria);
        Assert.Contains(results, r => r.MemberNames.Contains("TotalWordCount"));
    }

    [Fact]
    public void NegativeWords_FailsValidation()
    {
        var criteria = new WorksheetFilterCriteriaDto(-5, "en");
        var results = ValidateModel(criteria);
        Assert.Contains(results, r => r.MemberNames.Contains("TotalWordCount"));
    }

    [Fact]
    public void Over100Words_FailsValidation()
    {
        var criteria = new WorksheetFilterCriteriaDto(101, "en");
        var results = ValidateModel(criteria);
        Assert.Contains(results, r => r.MemberNames.Contains("TotalWordCount"));
    }

    [Fact]
    public void MissingLanguage_FailsValidation()
    {
        var criteria = new WorksheetFilterCriteriaDto(50, null!);
        var results = ValidateModel(criteria);
        Assert.Contains(results, r => r.MemberNames.Contains("Language"));
    }
}
