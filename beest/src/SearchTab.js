import { useCallback, useEffect, useState } from 'react';
import { searchCmudict } from './cmudictApi';
import { PronunciationRow } from './PronunciationRow';
import { readStoredLang, writeStoredLang } from './langStorage';

const PLACEHOLDER_EN = 'e.g. hello, car, dog';
const PLACEHOLDER_ES = 'e.g. casa, perro, sol';

export default function SearchTab() {
  const [query, setQuery] = useState('');
  const [mode, setMode] = useState('prefix');
  const [limit] = useState(50);
  const [lang, setLang] = useState(readStoredLang);
  const [results, setResults] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [lastMeta, setLastMeta] = useState(null);

  useEffect(() => {
    writeStoredLang(lang);
  }, [lang]);

  useEffect(() => {
    setResults([]);
    setLastMeta(null);
    setError(null);
  }, [lang]);

  const runSearch = useCallback(async () => {
    const q = query.trim();
    if (!q) return;

    setError(null);
    setLoading(true);
    setLastMeta(null);

    try {
      const data = await searchCmudict({ q, mode, limit, lang });
      setResults(data.results);
      setLastMeta({ query: data.query, mode: data.mode, lang });
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Search failed');
      setResults([]);
    } finally {
      setLoading(false);
    }
  }, [query, mode, limit, lang]);

  const onSubmit = (e) => {
    e.preventDefault();
    runSearch();
  };

  return (
    <>
      <p className="tab-subtitle">
        Search the current dictionary, identify syllable structure.
      </p>

      <form className="search-toolbar" onSubmit={onSubmit}>
        <label className="field">
          <span className="field-label">Word</span>
          <input
            type="search"
            className="search-input"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder={lang === 'es' ? PLACEHOLDER_ES : PLACEHOLDER_EN}
            autoComplete="off"
            aria-label={lang === 'es' ? 'Buscar palabra' : 'Search word'}
          />
        </label>

        <fieldset className="mode-fieldset">
          <legend className="mode-legend">Language</legend>
          <label className="radio-label">
            <input
              type="radio"
              name="search-lang"
              value="en"
              checked={lang === 'en'}
              onChange={() => setLang('en')}
            />
            English
          </label>
          <label className="radio-label">
            <input
              type="radio"
              name="search-lang"
              value="es"
              checked={lang === 'es'}
              onChange={() => setLang('es')}
            />
            Spanish
          </label>
        </fieldset>

        <fieldset className="mode-fieldset">
          <legend className="mode-legend">Match</legend>
          <label className="radio-label">
            <input
              type="radio"
              name="mode"
              value="prefix"
              checked={mode === 'prefix'}
              onChange={() => setMode('prefix')}
            />
            Prefix
          </label>
          <label className="radio-label">
            <input
              type="radio"
              name="mode"
              value="contains"
              checked={mode === 'contains'}
              onChange={() => setMode('contains')}
            />
            Contains
          </label>
        </fieldset>

        <button
          type="submit"
          className="search-button"
          disabled={loading || !query.trim()}
        >
          {loading ? 'Searching…' : 'Search'}
        </button>
      </form>

      <div className="status-area" role="status" aria-live="polite">
        {error && <p className="status-error">{error}</p>}
        {!error && loading && <p className="status-info">Searching…</p>}
        {!error && !loading && lastMeta && results.length === 0 && (
          <p className="status-info">No matches.</p>
        )}
        {!error && !loading && lastMeta && results.length > 0 && (
          <p className="status-meta">
            {results.length} result{results.length === 1 ? '' : 's'} for “
            {lastMeta.query}” ({lastMeta.mode},{' '}
            {lastMeta.lang === 'es' ? 'Español' : 'English'})
          </p>
        )}
      </div>

      <div className="results-scroll">
        <ul className="results-list">
          {results.map((item) => (
            <li key={`${item.word}-${item.pronunciations.join('|')}`} className="result-card">
              <div className="result-word">{item.word}</div>
              <ul className="pronunciation-list">
                {item.pronunciations.map((p) => (
                  <li key={p}>
                    <PronunciationRow pronunciation={p} />
                  </li>
                ))}
              </ul>
            </li>
          ))}
        </ul>
      </div>
    </>
  );
}
