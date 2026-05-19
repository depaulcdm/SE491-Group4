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
    string? SearchTerm = null,
    string? IncludedPhonemes = null,
    string? ExcludedPhonemes = null,
    string? IncludedSyllableStructure = null,
    string? ExcludedSyllableStructure = null,
    int? RandomSeed = null
);
