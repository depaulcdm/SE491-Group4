using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;

namespace BEEST;

public sealed class CmudictLexicon
{
    private static readonly Regex TrailingVariant = new(@"\(\d+\)$", RegexOptions.Compiled);

    private readonly Dictionary<string, List<string>> _byBaseWord = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _sortedBaseWords = [];

    public CmudictLexicon(IWebHostEnvironment env, string dictionaryPath, ILogger<CmudictLexicon> logger)
    {
        var path = ResolveDictionaryPath(env, dictionaryPath);

        if (!File.Exists(path))
            throw new FileNotFoundException(
                $"Dictionary file not found. Tried resolved path: {path}",
                path);

        Load(path);
        _sortedBaseWords = _byBaseWord.Keys.OrderBy(k => k, StringComparer.Ordinal).ToList();

        logger.LogInformation(
            "Loaded dictionary from {Path}: {WordCount} headwords, {PronunciationCount} pronunciation lines.",
            path,
            _byBaseWord.Count,
            _byBaseWord.Values.Sum(v => v.Count));
    }

    private static string ResolveDictionaryPath(IWebHostEnvironment env, string configuredPath)
    {
        if (Path.IsPathRooted(configuredPath))
            return configuredPath;

        var fromContentRoot = Path.GetFullPath(Path.Combine(env.ContentRootPath, configuredPath));
        if (File.Exists(fromContentRoot))
            return fromContentRoot;

        var fromBase = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, configuredPath));
        if (File.Exists(fromBase))
            return fromBase;

        return fromContentRoot;
    }

    private void Load(string path)
    {
        var encoding = Encoding.Latin1;

        foreach (var line in File.ReadLines(path, encoding))
        {
            var parsed = TryParseLine(line);
            if (parsed is null)
                continue;

            var (baseWord, phonemeString) = parsed.Value;
            if (!_byBaseWord.TryGetValue(baseWord, out var list))
            {
                list = [];
                _byBaseWord[baseWord] = list;
            }

            if (!list.Contains(phonemeString, StringComparer.Ordinal))
                list.Add(phonemeString);
        }
    }

    private static (string BaseWord, string Phonemes)? TryParseLine(string line)
    {
        line = line.Trim();
        if (line.Length == 0 || line.StartsWith(";;;", StringComparison.Ordinal))
            return null;

        var hash = line.IndexOf('#', StringComparison.Ordinal);
        if (hash >= 0)
            line = line[..hash].TrimEnd();

        var parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
            return null;

        var headword = parts[0];
        var phonemes = parts.AsSpan(1);
        var phonemeString = string.Join(' ', phonemes.ToArray());

        var baseWord = TrailingVariant.Replace(headword, string.Empty).ToLowerInvariant();

        return (baseWord, phonemeString);
    }

    public IReadOnlyList<CmudictSearchHit> Search(string query, SearchMode mode, int limit)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<CmudictSearchHit>();

        query = query.Trim().ToLowerInvariant();
        limit = Math.Clamp(limit, 1, 200);

        IEnumerable<string> candidates = mode switch
        {
            SearchMode.Prefix => _sortedBaseWords.Where(w => w.StartsWith(query, StringComparison.Ordinal)),
            SearchMode.Contains => _sortedBaseWords.Where(w => w.Contains(query, StringComparison.Ordinal)),
            _ => throw new ArgumentOutOfRangeException(nameof(mode)),
        };

        var hits = new List<CmudictSearchHit>(limit);
        foreach (var baseWord in candidates)
        {
            if (hits.Count >= limit)
                break;

            var prons = _byBaseWord[baseWord];
            hits.Add(new CmudictSearchHit(baseWord, prons.ToArray()));
        }

        return hits;
    }

    public WorksheetGenerateResponseDto GenerateWorksheet(
        WorksheetFilterCriteriaDto criteria,
        PhoneInventory phones)
    {
        var validationContext = new ValidationContext(criteria, null, null);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(
            criteria,
            validationContext,
            validationResults,
            validateAllProperties: true);

        if (!isValid)
            throw new ArgumentException("Invalid criteria");

        var lang = criteria.Language.Trim().ToLowerInvariant();
        var included = PronunciationAnalyzer.ParsePhonemeTokens(criteria.IncludedPhonemes);
        var excluded = PronunciationAnalyzer.ParsePhonemeTokens(criteria.ExcludedPhonemes);

        var matches = new List<WorksheetWordDto>();

        foreach (var baseWord in _sortedBaseWords)
        {
            var pronunciations = _byBaseWord[baseWord];

            if (IsWordExcluded(pronunciations, excluded))
                continue;

            var matchingPronunciation = FindFirstMatchingPronunciation(
                pronunciations,
                lang,
                criteria,
                included,
                phones);

            if (matchingPronunciation is not null)
                matches.Add(new WorksheetWordDto(baseWord, matchingPronunciation));
        }

        var sampled = SampleMatches(matches, criteria.TotalWordCount, criteria.RandomSeed);

        return new WorksheetGenerateResponseDto(
            criteria.TotalWordCount,
            matches.Count,
            sampled.Count,
            criteria.RandomSeed,
            sampled);
    }

    private static bool IsWordExcluded(
        IReadOnlyList<string> pronunciations,
        IReadOnlyList<string> excludedTokens)
    {
        if (excludedTokens.Count == 0)
            return false;

        return pronunciations.Any(pronunciation =>
            PronunciationAnalyzer.ContainsExcludedPhoneme(pronunciation, excludedTokens));
    }

    private static string? FindFirstMatchingPronunciation(
        IReadOnlyList<string> pronunciations,
        string lang,
        WorksheetFilterCriteriaDto criteria,
        IReadOnlyList<string> includedTokens,
        PhoneInventory phones)
    {
        foreach (var pronunciation in pronunciations)
        {
            if (!PronunciationAnalyzer.ContainsAnyPhoneme(pronunciation, includedTokens))
                continue;

            if (criteria.SyllableCount is int syllableCount &&
                !PronunciationAnalyzer.MatchesSyllableCount(pronunciation, syllableCount))
                continue;

            if (!string.IsNullOrWhiteSpace(criteria.IncludedSyllableStructure) &&
                !PronunciationAnalyzer.MatchesCvStructure(
                    lang,
                    pronunciation,
                    criteria.IncludedSyllableStructure,
                    phones))
                continue;

            return pronunciation;
        }

        return null;
    }

    private static List<WorksheetWordDto> SampleMatches(
        IReadOnlyList<WorksheetWordDto> matches,
        int totalWordCount,
        int randomSeed)
    {
        if (matches.Count <= totalWordCount)
            return matches.ToList();

        var pool = matches.ToList();
        var rng = new Random(randomSeed);

        for (var i = pool.Count - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        return pool.Take(totalWordCount).ToList();
    }
}

public enum SearchMode
{
    Prefix,
    Contains,
}

public sealed record CmudictSearchHit(string Word, IReadOnlyList<string> Pronunciations);
