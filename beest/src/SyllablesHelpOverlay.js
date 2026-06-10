import { useEffect, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import { PHONEME_HELP } from './data/syllableHelpData';

function PhonemeTable({ rows }) {
  return (
    <table className="syllables-help-table">
      <thead>
        <tr>
          <th scope="col">Sound</th>
          <th scope="col">Example word</th>
        </tr>
      </thead>
      <tbody>
        {rows.map(({ phoneme, word }) => (
          <tr key={phoneme}>
            <td className="syllables-help-pattern">{phoneme}</td>
            <td className="syllables-help-word">{word}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

export default function SyllablesHelpOverlay({ initialLang = 'en', onClose }) {
  const [lang, setLang] = useState(initialLang === 'es' ? 'es' : 'en');
  const closeButtonRef = useRef(null);
  const { vowels = [], consonants = [] } = PHONEME_HELP[lang] ?? {};

  useEffect(() => {
    closeButtonRef.current?.focus();

    const onKeyDown = (event) => {
      if (event.key === 'Escape') onClose();
    };

    document.addEventListener('keydown', onKeyDown);
    return () => document.removeEventListener('keydown', onKeyDown);
  }, [onClose]);

  return createPortal(
    <div className="syllables-help-root">
      <button
        type="button"
        className="syllables-help-backdrop"
        aria-label="Close sound help"
        onClick={onClose}
      />
      <div
        className="syllables-help-panel"
        role="dialog"
        aria-modal="true"
        aria-labelledby="syllables-help-title"
      >
        <div className="syllables-help-header">
          <h2 id="syllables-help-title" className="syllables-help-title">
            Sounds
          </h2>
          <button
            ref={closeButtonRef}
            type="button"
            className="syllables-help-close secondary-button"
            onClick={onClose}
            aria-label="Close"
          >
            ×
          </button>
        </div>

        <fieldset className="mode-fieldset syllables-help-lang">
          <legend className="mode-legend">Language</legend>
          <label className="radio-label">
            <input
              type="radio"
              name="syllables-help-lang"
              value="en"
              checked={lang === 'en'}
              onChange={() => setLang('en')}
            />
            English
          </label>
          <label className="radio-label">
            <input
              type="radio"
              name="syllables-help-lang"
              value="es"
              checked={lang === 'es'}
              onChange={() => setLang('es')}
            />
            Spanish
          </label>
        </fieldset>
        <p></p>

        <details className="syllables-help-section" open>
          <summary className="syllables-help-section-toggle">Vowels</summary>
          <div className="syllables-help-table-wrap">
            <PhonemeTable rows={vowels} />
          </div>
        </details>

        <details className="syllables-help-section" open>
          <summary className="syllables-help-section-toggle">Consonants</summary>
          <div className="syllables-help-table-wrap">
            <PhonemeTable rows={consonants} />
          </div>
        </details>
      </div>
    </div>,
    document.body
  );
}
