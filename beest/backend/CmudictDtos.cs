using System.ComponentModel.DataAnnotations;

namespace BEEST;

public sealed record CmudictSearchResponseDto(
    string Query,
    string Mode,
    IReadOnlyList<CmudictSearchHitDto> Results);

public sealed record CmudictSearchHitDto(
    string Word,
    IReadOnlyList<string> Pronunciations);

public sealed record WorksheetFilterCriteriaDto(
    [property: Required] [property: Range(1, 100)] int TotalWordCount,
    [property: Required] string Language,
    [property: Required] int RandomSeed,
    string? IncludedPhonemes = null,
    string? ExcludedPhonemes = null,
    [property: Range(1, 20)] int? SyllableCount = null,
    [property: RegularExpression(@"^[CVcv]+$")] string? IncludedSyllableStructure = null
);

public sealed record WorksheetGenerateResponseDto(
    int RequestedWordCount,
    int TotalMatchesFound,
    int ReturnedWordCount,
    int RandomSeed,
    IReadOnlyList<WorksheetWordDto> Results);

public sealed record WorksheetWordDto(
    string Word,
    string Pronunciation);
