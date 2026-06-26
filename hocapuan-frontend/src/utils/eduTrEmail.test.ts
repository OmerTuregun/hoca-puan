import { describe, expect, it } from 'vitest'
import { getEduTrEmailWarning, parseApiErrorMessage } from './eduTrEmail'

describe('getEduTrEmailWarning', () => {
  it('returns null for empty or whitespace-only email', () => {
    expect(getEduTrEmailWarning(undefined)).toBeNull()
    expect(getEduTrEmailWarning('')).toBeNull()
    expect(getEduTrEmailWarning('   ')).toBeNull()
  })

  it('returns null for valid .edu.tr emails', () => {
    expect(getEduTrEmailWarning('ornek@uni.edu.tr')).toBeNull()
    expect(getEduTrEmailWarning('  student@METU.EDU.TR  ')).toBeNull()
    expect(getEduTrEmailWarning('a@b.edu.tr')).toBeNull()
  })

  it('returns warning for non-.edu.tr emails', () => {
    const warning = getEduTrEmailWarning('user@gmail.com')
    expect(warning).toBe('Sadece .edu.tr uzantılı üniversite e-postaları kabul edilmektedir.')
  })

  it('rejects emails that contain but do not end with .edu.tr', () => {
    expect(getEduTrEmailWarning('fake.edu.tr@gmail.com')).not.toBeNull()
    expect(getEduTrEmailWarning('user@edu.tr')).not.toBeNull()
  })
})

describe('parseApiErrorMessage', () => {
  const fallback = 'Varsayılan hata'

  it('returns fallback when no response data', () => {
    expect(parseApiErrorMessage({}, fallback)).toBe(fallback)
    expect(parseApiErrorMessage({ response: {} }, fallback)).toBe(fallback)
  })

  it('extracts message field', () => {
    const error = { response: { data: { message: 'API mesajı' } } }
    expect(parseApiErrorMessage(error, fallback)).toBe('API mesajı')
  })

  it('extracts first email validation error', () => {
    const error = {
      response: {
        data: {
          errors: { Email: ['Geçersiz e-posta'] },
        },
      },
    }
    expect(parseApiErrorMessage(error, fallback)).toBe('Geçersiz e-posta')
  })

  it('extracts password validation error', () => {
    const error = {
      response: {
        data: {
          errors: { Password: ['Şifre zayıf'] },
        },
      },
    }
    expect(parseApiErrorMessage(error, fallback)).toBe('Şifre zayıf')
  })

  it('falls back to title when no message or field errors', () => {
    const error = { response: { data: { title: 'Bad Request' } } }
    expect(parseApiErrorMessage(error, fallback)).toBe('Bad Request')
  })
})
