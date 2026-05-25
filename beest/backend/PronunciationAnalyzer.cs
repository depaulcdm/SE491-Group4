namespace BEEST;

public static class PronunciationAnalyzer
{
    public static int CountSyllables(string pronunciation)
    {
        if (string.IsNullOrWhiteSpace(pronunciation))
            return 0;

        return pronunciation
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Count(HasStressDigit);
    }

    public static string ToCvPattern(string lang, string pronunciation, PhoneInventory inventory)
    {
        if (string.IsNullOrWhiteSpace(pronunciation))
            return string.Empty;

        var phones = pronunciation.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(phones.Select(phone => inventory.Classify(lang, phone)));
    }

    public static bool MatchesSyllableCount(string pronunciation, int exactCount)
        => CountSyllables(pronunciation) == exactCount;

    public static bool MatchesCvStructure(
        string lang,
        string pronunciation,
        string requiredPattern,
        PhoneInventory inventory)
    {
        if (string.IsNullOrWhiteSpace(requiredPattern))
            return true;

        var actual = ToCvPattern(lang, pronunciation, inventory);
        return actual.Equals(requiredPattern.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    public static bool ContainsAnyPhoneme(string pronunciation, IReadOnlyList<string> tokens)
    {
        if (tokens.Count == 0)
            return true;

        if (string.IsNullOrWhiteSpace(pronunciation))
            return false;

        return tokens.Any(token =>
            pronunciation.Contains(token, StringComparison.OrdinalIgnoreCase));
    }

    public static bool ContainsExcludedPhoneme(string pronunciation, IReadOnlyList<string> tokens)
    {
        if (tokens.Count == 0 || string.IsNullOrWhiteSpace(pronunciation))
            return false;

        return tokens.Any(token =>
            pronunciation.Contains(token, StringComparison.OrdinalIgnoreCase));
    }

    public static IReadOnlyList<string> ParsePhonemeTokens(string? phonemes)
    {
        if (string.IsNullOrWhiteSpace(phonemes))
            return Array.Empty<string>();

        return phonemes
            .Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }

    private static bool HasStressDigit(string phone)
    {
        if (phone.Length == 0)
            return false;

        var last = phone[^1];
        return last is >= '0' and <= '2';
    }
}
