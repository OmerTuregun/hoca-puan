/** Hoca kartında gösterilecek ad — ünvan tekrarını önler. */
export function displayProfessorName(p: {
  fullName?: string
  firstName?: string
  lastName?: string
  title?: string
}): string {
  const fromParts = `${p.firstName ?? ''} ${p.lastName ?? ''}`.trim()
  if (fromParts) return fromParts

  const full = (p.fullName ?? '').trim()
  const title = (p.title ?? '').trim()
  if (title && full.toLowerCase().startsWith(title.toLowerCase()))
    return full.slice(title.length).trim()

  return full || '—'
}

export function professorInitials(p: {
  fullName?: string
  firstName?: string
  lastName?: string
  title?: string
}): string {
  const name = displayProfessorName(p)
  const parts = name.split(/\s+/).filter(Boolean)
  const a = parts[0]?.[0] ?? ''
  const b = parts.length > 1 ? parts[parts.length - 1][0] : ''
  return `${a}${b}`.toUpperCase()
}
