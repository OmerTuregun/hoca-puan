export type ReviewSubmitErrorKind = 'moderation' | 'general' | 'forbidden'

export interface ParsedReviewSubmitError {
  message: string
  kind: ReviewSubmitErrorKind
}

export function parseReviewSubmitError(error: unknown): ParsedReviewSubmitError {
  const err = error as {
    response?: { status?: number; data?: { message?: string } }
    request?: unknown
  }

  const status = err.response?.status
  const apiMessage = err.response?.data?.message?.trim()

  if (status === 403) {
    return {
      message: 'Bu yorumu düzenleme yetkiniz yok.',
      kind: 'forbidden',
    }
  }

  if (status === 400 && apiMessage) {
    return { message: apiMessage, kind: 'moderation' }
  }

  if (status === 429 && apiMessage) {
    return { message: apiMessage, kind: 'general' }
  }

  if (!status && err.request) {
    return {
      message: 'Bağlantı hatası. Lütfen tekrar deneyin.',
      kind: 'general',
    }
  }

  return {
    message: 'Bir şeyler ters gitti. Lütfen tekrar deneyin.',
    kind: 'general',
  }
}
