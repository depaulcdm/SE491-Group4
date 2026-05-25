namespace BEEST;

public sealed class LexiconOptions
{
    public const string SectionName = "Lexicon";

    public string EnglishDictionaryPath { get; set; } = "lang/english/cmudict.dict";

    public string SpanishDictionaryPath { get; set; } = "lang/spanish/spanish.dict";

    public string EnglishPhonesPath { get; set; } = "lang/english/cmudict.phones";

    public string SpanishPhonesPath { get; set; } = "lang/spanish/spanish.phone";
}
