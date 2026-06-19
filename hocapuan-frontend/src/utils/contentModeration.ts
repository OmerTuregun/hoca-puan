/**
 * UX amaçlı anlık uyarı — kesin karar backend'de verilir.
 * banned-words.tr.json backend ile senkron (public/moderation/banned-words.tr.json).
 */

const TR_LOCALE = 'tr-TR'

const LEET_MAP: Record<string, string> = {
  '4': 'a', '@': 'a',
  '3': 'e',
  '1': 'i', '!': 'i', '|': 'i',
  '0': 'o',
  '5': 's', '$': 's',
  '7': 't',
}

type PatternSet = { words: RegExp[]; phrases: RegExp[] }

let cachedPatterns: PatternSet | null = null
let loadPromise: Promise<PatternSet> | null = null

export async function loadBannedWordPatterns(): Promise<RegExp[]> {
  const { words, phrases } = await loadPatternSet()
  return [...words, ...phrases]
}

async function loadPatternSet(): Promise<PatternSet> {
  if (cachedPatterns) return cachedPatterns
  if (!loadPromise) {
    loadPromise = fetch('/moderation/banned-words.tr.json')
      .then(r => r.json() as Promise<Record<string, unknown>>)
      .then(data => {
        const entries = Object.entries(data)
          .filter(([key, value]) => !key.startsWith('_') && Array.isArray(value))
          .flatMap(([, value]) => value as string[])

        const words: RegExp[] = []
        const phrases: RegExp[] = []

        for (const entry of entries) {
          const trimmed = entry.trim()
          if (!trimmed) continue

          if (isMultiWordPhrase(trimmed)) {
            const normalized = normalizePhrase(trimmed)
            if (normalized.split(/\s+/).length >= 2)
              phrases.push(buildPhraseRegex(normalized))
          } else {
            const normalized = normalizeWord(trimmed)
            if (normalized.length >= 2)
              words.push(buildWordRegex(normalized))
          }
        }

        cachedPatterns = { words, phrases }
        return cachedPatterns
      })
      .catch(() => {
        cachedPatterns = { words: [], phrases: [] }
        return cachedPatterns
      })
  }
  return loadPromise
}

export function mayContainBannedContent(text: string, patterns: RegExp[]): boolean {
  if (!text.trim() || patterns.length === 0) return false
  const variants = buildMatchVariants(text)
  return patterns.some(p => variants.some(v => matchesPattern(p, v)))
}

function matchesPattern(pattern: RegExp, variant: string): boolean {
  if (pattern.test(variant)) return true
  return variant.split(/\s+/).some(token => token && pattern.test(token))
}

function isMultiWordPhrase(entry: string): boolean {
  return entry.split(/\s+/).filter(Boolean).length > 1
}

function normalizePhrase(phrase: string): string {
  return phrase
    .split(/\s+/)
    .map(w => normalizeWord(w))
    .filter(Boolean)
    .join(' ')
}

function buildWordRegex(normalized: string): RegExp {
  return new RegExp(`(?<![\\p{L}\\d])${escapeRegex(normalized)}(?![\\p{L}\\d])`, 'iu')
}

function buildPhraseRegex(normalizedPhrase: string): RegExp {
  const parts = normalizedPhrase.split(/\s+/).filter(Boolean)
  const pattern = parts.map(escapeRegex).join('[\\W_]*')
  return new RegExp(pattern, 'iu')
}

function buildMatchVariants(text: string): string[] {
  const lowered = text.toLocaleLowerCase(TR_LOCALE)
  const variants = new Set<string>([
    lowered,
    foldTurkish(lowered),
    applyLeet(foldTurkish(lowered)),
    joinSeparatedLetters(lowered),
    lettersOnlyPerToken(lowered),
    collapseRepeated(lowered),
    collapseRepeated(applyLeet(foldTurkish(lowered))),
    collapseRepeated(lettersOnlyPerToken(lowered)),
  ])
  return [...variants]
}

function normalizeWord(word: string): string {
  return collapseRepeated(applyLeet(foldTurkish(word.toLocaleLowerCase(TR_LOCALE))))
}

function foldTurkish(text: string): string {
  return text
    .replace(/ı/g, 'i').replace(/İ/g, 'i').replace(/I/g, 'i')
    .replace(/ş/g, 's').replace(/Ş/g, 's')
    .replace(/ç/g, 'c').replace(/Ç/g, 'c')
    .replace(/ğ/g, 'g').replace(/Ğ/g, 'g')
    .replace(/ü/g, 'u').replace(/Ü/g, 'u')
    .replace(/ö/g, 'o').replace(/Ö/g, 'o')
}

function applyLeet(text: string): string {
  return [...text].map(ch => LEET_MAP[ch] ?? ch).join('')
}

function joinSeparatedLetters(text: string): string {
  return text
    .split(/\s+/)
    .map(token => token.replace(/([\p{L}])[\*._\-/\\|]+(?=[\p{L}])/gu, '$1'))
    .join(' ')
}

function lettersOnlyPerToken(text: string): string {
  return text
    .split(/\s+/)
    .map(token => [...token].filter(ch => /\p{L}/u.test(ch)).join('').toLocaleLowerCase(TR_LOCALE))
    .filter(Boolean)
    .join(' ')
}

function collapseRepeated(text: string): string {
  return text.replace(/(.)\1+/g, '$1')
}

function escapeRegex(value: string): string {
  return value.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')
}
