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
}

public enum SearchMode
{
    Prefix,
    Contains,
}

public sealed record CmudictSearchHit(string Word, IReadOnlyList<string> Pronunciations);
