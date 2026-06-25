const TR_MAP: Record<string, string> = {
  ş: 's', Ş: 'S',
  ı: 'i', İ: 'I',
  ğ: 'g', Ğ: 'G',
  ü: 'u', Ü: 'U',
  ö: 'o', Ö: 'O',
  ç: 'c', Ç: 'C',
}

/** Turkish-aware slugify: normalizes chars, lowercases, hyphenates. */
export function slugify(text: string): string {
  return text
    .split('')
    .map(c => TR_MAP[c] ?? c)
    .join('')
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLowerCase()
    .replace(/[^a-z0-9\s-]/g, '')
    .trim()
    .replace(/[\s_]+/g, '-')
    .replace(/-+/g, '-')
}
