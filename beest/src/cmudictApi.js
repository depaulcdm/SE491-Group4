const defaultBase = 'http://localhost:5087';

function getBaseUrl() {
  const raw = process.env.REACT_APP_API_BASE_URL ?? defaultBase;
  return raw.replace(/\/$/, '');
}

async function parseJsonResponse(response) {
  const text = await response.text();
  try {
    return text ? JSON.parse(text) : {};
  } catch {
    return { raw: text };
  }
}

function formatApiError(body, fallback) {
  if (body?.error) return body.error;

  if (body?.errors && typeof body.errors === 'object') {
    const messages = Object.entries(body.errors).flatMap(([field, values]) => {
      const list = Array.isArray(values) ? values : [values];
      return list.map((message) => `${field}: ${message}`);
    });
    if (messages.length > 0) return messages.join(' ');
  }

  return body?.title || (typeof body?.raw === 'string' ? body.raw : null) || fallback;
}

/**
 * @param {{ q: string, mode: 'prefix' | 'contains', limit?: number, lang?: 'en' | 'es' }} params
 * @returns {Promise<{ query: string, mode: string, results: Array<{ word: string, pronunciations: string[] }> }>}
 */
export async function searchCmudict({ q, mode, limit = 50, lang = 'en' }) {
  const base = getBaseUrl();
  const url = new URL('/api/cmudict/search', `${base}/`);
  url.searchParams.set('q', q);
  url.searchParams.set('mode', mode);
  url.searchParams.set('limit', String(limit));
  url.searchParams.set('lang', lang);

  const response = await fetch(url.toString(), {
    method: 'GET',
    headers: { Accept: 'application/json' },
  });

  const body = await parseJsonResponse(response);

  if (!response.ok) {
    throw new Error(formatApiError(body, `Request failed (${response.status})`));
  }

  return {
    query: body.query ?? q,
    mode: body.mode ?? mode,
    results: Array.isArray(body.results) ? body.results : [],
  };
}

/**
 * @param {{
 *   totalWordCount: number,
 *   language: 'en' | 'es',
 *   randomSeed: number,
 *   includedPhonemes?: string | null,
 *   excludedPhonemes?: string | null,
 *   syllableCount?: number | null,
 *   includedSyllableStructure?: string | null,
 * }} criteria
 * @returns {Promise<{
 *   requestedWordCount: number,
 *   totalMatchesFound: number,
 *   returnedWordCount: number,
 *   randomSeed: number,
 *   results: Array<{ word: string, pronunciation: string }>,
 * }>}
 */
export async function generateWorksheet(criteria) {
  const base = getBaseUrl();
  const response = await fetch(`${base}/api/worksheet/generate`, {
    method: 'POST',
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      totalWordCount: criteria.totalWordCount,
      language: criteria.language,
      randomSeed: criteria.randomSeed,
      includedPhonemes: criteria.includedPhonemes,
      excludedPhonemes: criteria.excludedPhonemes,
      syllableCount: criteria.syllableCount,
      includedSyllableStructure: criteria.includedSyllableStructure,
    }),
  });

  const body = await parseJsonResponse(response);

  if (!response.ok) {
    throw new Error(formatApiError(body, `Request failed (${response.status})`));
  }

  return {
    requestedWordCount: body.requestedWordCount ?? criteria.totalWordCount,
    totalMatchesFound: body.totalMatchesFound ?? 0,
    returnedWordCount: body.returnedWordCount ?? 0,
    randomSeed: body.randomSeed ?? criteria.randomSeed,
    results: Array.isArray(body.results) ? body.results : [],
  };
}
