export const LANG_STORAGE_KEY = 'beest-lang';

export function readStoredLang() {
  try {
    const raw = localStorage.getItem(LANG_STORAGE_KEY);
    if (raw === 'en' || raw === 'es') return raw;
  } catch {
    /* ignore */
  }
  return 'en';
}

export function writeStoredLang(lang) {
  try {
    localStorage.setItem(LANG_STORAGE_KEY, lang);
  } catch {
    /* ignore */
  }
}
