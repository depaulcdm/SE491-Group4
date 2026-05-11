const defaultBase = 'http://localhost:5087';

function getBaseUrl() {
  const raw = process.env.REACT_APP_API_BASE_URL ?? defaultBase;
  return raw.replace(/\/$/, '');
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

  const text = await response.text();
  let body;
  try {
    body = text ? JSON.parse(text) : {};
  } catch {
    body = { raw: text };
  }

  if (!response.ok) {
    const msg =
      body?.error ||
      body?.title ||
      (typeof body?.raw === 'string' ? body.raw : null) ||
      `Request failed (${response.status})`;
    throw new Error(msg);
  }

  return {
    query: body.query ?? q,
    mode: body.mode ?? mode,
    results: Array.isArray(body.results) ? body.results : [],
  };
}
