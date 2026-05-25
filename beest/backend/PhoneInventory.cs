using Microsoft.Extensions.Options;

namespace BEEST;

public class PhoneInventory
{
    private readonly HashSet<string> _englishVowels;
    private readonly HashSet<string> _spanishVowels;

    public PhoneInventory(IWebHostEnvironment env, IOptions<LexiconOptions> options)
        : this(
            ResolvePhoneFilePath(env, options.Value.EnglishPhonesPath),
            ResolvePhoneFilePath(env, options.Value.SpanishPhonesPath))
    {
    }

    public PhoneInventory(string englishPhonesPath, string spanishPhonesPath)
    {
        _englishVowels = LoadVowels(englishPhonesPath);
        _spanishVowels = LoadVowels(spanishPhonesPath);
    }

    public bool IsVowel(string lang, string phone)
    {
        var basePhone = StripStressDigits(phone);
        return GetVowelSet(lang).Contains(basePhone);
    }

    public char Classify(string lang, string phone)
        => IsVowel(lang, phone) ? 'V' : 'C';

    private HashSet<string> GetVowelSet(string lang)
    {
        return lang.Trim().ToLowerInvariant() switch
        {
            "en" => _englishVowels,
            "es" => _spanishVowels,
            _ => throw new ArgumentException($"Unsupported language '{lang}'.", nameof(lang)),
        };
    }

    private static HashSet<string> LoadVowels(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Phone inventory file not found: {path}", path);

        var vowels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in File.ReadLines(path))
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
                continue;

            var parts = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
                continue;

            if (parts[1].Equals("vowel", StringComparison.OrdinalIgnoreCase))
                vowels.Add(parts[0]);
        }

        return vowels;
    }

    public static string StripStressDigits(string phone)
    {
        if (phone.Length == 0)
            return phone;

        var last = phone[^1];
        return last is >= '0' and <= '2' ? phone[..^1] : phone;
    }

    private static string ResolvePhoneFilePath(IWebHostEnvironment env, string configuredPath)
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
}
