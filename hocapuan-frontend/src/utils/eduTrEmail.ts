/** ASP.NET model validation veya API message alanından kullanıcıya gösterilecek metni çıkarır. */
export function parseApiErrorMessage(error: unknown, fallback: string): string {
  const data = (error as { response?: { data?: Record<string, unknown> } })?.response?.data
  if (!data) return fallback

  if (typeof data.message === 'string' && data.message) return data.message

  const errors = data.errors as Record<string, string[]> | undefined
  if (errors) {
    const emailErrors = errors.Email ?? errors.email
    if (emailErrors?.[0]) return emailErrors[0]

    const passwordErrors = errors.Password ?? errors.password ?? errors.NewPassword ?? errors.newPassword
    if (passwordErrors?.[0]) return passwordErrors[0]

    const first = Object.values(errors).flat()[0]
    if (typeof first === 'string' && first) return first
  }

  if (typeof data.title === 'string' && data.title) return data.title

  return fallback
}

/** E-posta .edu.tr ile bitmiyorsa uyarı metni döner; boşsa null. */
export function getEduTrEmailWarning(email: string | undefined): string | null {
  const trimmed = email?.trim()
  if (!trimmed) return null
  if (!trimmed.toLowerCase().endsWith('.edu.tr')) {
    return 'Sadece .edu.tr uzantılı üniversite e-postaları kabul edilmektedir.'
  }
  return null
}
