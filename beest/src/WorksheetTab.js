import { useCallback, useEffect, useState } from 'react';
import { generateWorksheet } from './cmudictApi';
import { PronunciationRow } from './PronunciationRow';
import { readStoredLang, writeStoredLang } from './langStorage';

const MAX_INT32 = 2_147_483_647;

function clampRandomSeed(value) {
  const numeric = Math.trunc(Number(value));
  if (!Number.isFinite(numeric)) return 0;
  return Math.min(Math.max(0, numeric), MAX_INT32);
}

function createRandomSeed() {
  if (typeof crypto !== 'undefined' && crypto.getRandomValues) {
    const buffer = new Uint32Array(1);
    crypto.getRandomValues(buffer);
    return buffer[0] % (MAX_INT32 + 1);
  }
  return Math.floor(Math.random() * (MAX_INT32 + 1));
}

export default function WorksheetTab() {
  const [lang, setLang] = useState(readStoredLang);
  const [totalWordCount, setTotalWordCount] = useState(20);
  const [includedPhonemes, setIncludedPhonemes] = useState('');
  const [excludedPhonemes, setExcludedPhonemes] = useState('');
  const [syllableCount, setSyllableCount] = useState('');
  const [syllableStructure, setSyllableStructure] = useState('');
  const [randomSeed, setRandomSeed] = useState(createRandomSeed);
  const [results, setResults] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    writeStoredLang(lang);
  }, [lang]);

  useEffect(() => {
    setResults(null);
    setError(null);
  }, [lang]);

  const wordCountValid =
    Number.isFinite(totalWordCount) && totalWordCount >= 1 && totalWordCount <= 100;

  const runGenerate = useCallback(async () => {
    if (!wordCountValid) return;

    const seed = clampRandomSeed(randomSeed);
    if (!Number.isFinite(Number(randomSeed))) {
      setError('Random seed must be a valid number.');
      return;
    }

    setError(null);
    setLoading(true);
    setResults(null);

    try {
      const data = await generateWorksheet({
        totalWordCount: Number(totalWordCount),
        language: lang,
        randomSeed: seed,
        includedPhonemes: includedPhonemes.trim() || null,
        excludedPhonemes: excludedPhonemes.trim() || null,
        syllableCount: syllableCount === '' ? null : Number(syllableCount),
        includedSyllableStructure: syllableStructure.trim() || null,
      });
      setResults(data);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Worksheet generation failed');
    } finally {
      setLoading(false);
    }
  }, [
    wordCountValid,
    totalWordCount,
    lang,
    randomSeed,
    includedPhonemes,
    excludedPhonemes,
    syllableCount,
    syllableStructure,
  ]);

  const onSubmit = (e) => {
    e.preventDefault();
    runGenerate();
  };

  return (
    <>
      <p className="tab-subtitle">
        Generate a worksheet from filter criteria.
      </p>

      <form className="search-toolbar worksheet-form" onSubmit={onSubmit}>
        <fieldset className="mode-fieldset field-full">
          <legend className="mode-legend">Language</legend>
          <label className="radio-label">
            <input
              type="radio"
              name="worksheet-lang"
              value="en"
              checked={lang === 'en'}
              onChange={() => setLang('en')}
            />
            English
          </label>
          <label className="radio-label">
            <input
              type="radio"
              name="worksheet-lang"
              value="es"
              checked={lang === 'es'}
              onChange={() => setLang('es')}
            />
            Spanish
          </label>
        </fieldset>

        <label className="field">
          <span className="field-label">Number of words</span>
          <input
            type="number"
            className="search-input"
            min={1}
            max={100}
            value={totalWordCount}
            onChange={(e) => setTotalWordCount(Number(e.target.value))}
            aria-label="Number of words"
          />
        </label>

        <label className="field">
          <span className="field-label">Target sounds</span>
          <input
            type="text"
            className="search-input"
            value={includedPhonemes}
            onChange={(e) => setIncludedPhonemes(e.target.value)}
            placeholder="K AE T"
            autoComplete="off"
            aria-label="Target sounds"
          />
        </label>

        <label className="field">
          <span className="field-label">Excluded sounds</span>
          <input
            type="text"
            className="search-input"
            value={excludedPhonemes}
            onChange={(e) => setExcludedPhonemes(e.target.value)}
            placeholder="EH OW"
            autoComplete="off"
            aria-label="Excluded sounds"
          />
        </label>

        <label className="field">
          <span className="field-label">Syllable count</span>
          <input
            type="number"
            className="search-input"
            min={1}
            max={20}
            value={syllableCount}
            onChange={(e) => setSyllableCount(e.target.value)}
            placeholder="Optional"
            aria-label="Syllable count"
          />
        </label>

        <label className="field">
          <span className="field-label">Syllable structure</span>
          <input
            type="text"
            className="search-input"
            value={syllableStructure}
            onChange={(e) => setSyllableStructure(e.target.value)}
            placeholder="CVCV"
            autoComplete="off"
            aria-label="Syllable structure"
          />
        </label>

        <div className="field seed-row">
          <label className="field-label" htmlFor="random-seed">
            Random seed
          </label>
          <div className="seed-controls">
            <input
              id="random-seed"
              type="number"
              className="search-input"
              min={0}
              max={MAX_INT32}
              value={randomSeed}
              onChange={(e) => setRandomSeed(clampRandomSeed(e.target.value))}
              aria-label="Random seed"
            />
            <button
              type="button"
              className="secondary-button"
              onClick={() => setRandomSeed(createRandomSeed())}
            >
              New seed
            </button>
          </div>
        </div>

        <button
          type="submit"
          className="search-button field-full"
          disabled={loading || !wordCountValid}
        >
          {loading ? 'Generating…' : 'Generate'}
        </button>
      </form>

      <div className="status-area" role="status" aria-live="polite">
        {error && <p className="status-error">{error}</p>}
        {!error && loading && <p className="status-info">Generating worksheet…</p>}
        {!error && !loading && results && (
          <p className="status-meta">
            Found {results.returnedWordCount} of {results.requestedWordCount} requested
            ({results.totalMatchesFound} total matches, seed {results.randomSeed})
          </p>
        )}
      </div>

      <div className="results-scroll">
        {results && results.results.length > 0 && (
          <ol className="worksheet-results-list">
            {results.results.map((item, index) => (
              <li key={`${index}-${item.word}-${item.pronunciation}`} className="result-card">
                <div className="result-word">
                  {index + 1}. {item.word}
                </div>
                <PronunciationRow pronunciation={item.pronunciation} />
              </li>
            ))}
          </ol>
        )}
      </div>
    </>
  );
}
