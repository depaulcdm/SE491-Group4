using Microsoft.Extensions.Options;

namespace BEEST;

public sealed class LexiconRegistry
{
    public CmudictLexicon English { get; }
    public CmudictLexicon Spanish { get; }

    public LexiconRegistry(
        IWebHostEnvironment env,
        IOptions<LexiconOptions> options,
        ILoggerFactory loggerFactory)
    {
        var opts = options.Value;
        var logger = loggerFactory.CreateLogger<CmudictLexicon>();
        English = new CmudictLexicon(env, opts.EnglishDictionaryPath, logger);
        Spanish = new CmudictLexicon(env, opts.SpanishDictionaryPath, logger);
    }

    public bool TryGetLexicon(string lang, out CmudictLexicon? lexicon)
    {
        var key = lang.Trim().ToLowerInvariant();
        lexicon = key switch
        {
            "en" => English,
            "es" => Spanish,
            _ => null,
        };

        return lexicon is not null;
    }
}
