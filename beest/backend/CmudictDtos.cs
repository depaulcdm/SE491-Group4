namespace BEEST;

public sealed record CmudictSearchResponseDto(
    string Query,
    string Mode,
    IReadOnlyList<CmudictSearchHitDto> Results);

public sealed record CmudictSearchHitDto(
    string Word,
    IReadOnlyList<string> Pronunciations);
