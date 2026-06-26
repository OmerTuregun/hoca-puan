import { describe, expect, it } from 'vitest'
import { parseReviewSubmitError } from './reviewSubmitError'

function axiosError(status?: number, message?: string, hasRequest = false) {
  return {
    response: status !== undefined ? { status, data: { message } } : undefined,
    request: hasRequest ? {} : undefined,
  }
}

describe('parseReviewSubmitError', () => {
  it('maps 403 to forbidden kind with fixed message', () => {
    const result = parseReviewSubmitError(axiosError(403, 'ignored'))
    expect(result).toEqual({
      message: 'Bu yorumu düzenleme yetkiniz yok.',
      kind: 'forbidden',
    })
  })

  it('maps 400 with API message to moderation kind', () => {
    const result = parseReviewSubmitError(axiosError(400, 'Uygunsuz içerik'))
    expect(result).toEqual({
      message: 'Uygunsuz içerik',
      kind: 'moderation',
    })
  })

  it('maps 400 without API message to general fallback', () => {
    const result = parseReviewSubmitError(axiosError(400))
    expect(result.kind).toBe('general')
    expect(result.message).toBe('Bir şeyler ters gitti. Lütfen tekrar deneyin.')
  })

  it('maps 401 to general fallback (no dedicated handler)', () => {
    const result = parseReviewSubmitError(axiosError(401, 'Unauthorized'))
    expect(result).toEqual({
      message: 'Bir şeyler ters gitti. Lütfen tekrar deneyin.',
      kind: 'general',
    })
  })

  it('maps 429 with API message to general kind', () => {
    const result = parseReviewSubmitError(axiosError(429, 'Çok fazla istek'))
    expect(result).toEqual({
      message: 'Çok fazla istek',
      kind: 'general',
    })
  })

  it('maps 500 to general fallback', () => {
    const result = parseReviewSubmitError(axiosError(500, 'Internal Server Error'))
    expect(result).toEqual({
      message: 'Bir şeyler ters gitti. Lütfen tekrar deneyin.',
      kind: 'general',
    })
  })

  it('maps network errors (request without response) to connection message', () => {
    const result = parseReviewSubmitError(axiosError(undefined, undefined, true))
    expect(result).toEqual({
      message: 'Bağlantı hatası. Lütfen tekrar deneyin.',
      kind: 'general',
    })
  })

  it('maps unknown errors to general fallback', () => {
    const result = parseReviewSubmitError(new Error('boom'))
    expect(result).toEqual({
      message: 'Bir şeyler ters gitti. Lütfen tekrar deneyin.',
      kind: 'general',
    })
  })
})
