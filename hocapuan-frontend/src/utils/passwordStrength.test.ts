import { describe, expect, it } from 'vitest'
import {
  checkPasswordRules,
  isStrongPassword,
  STRONG_PASSWORD_MESSAGE,
} from './passwordStrength'

describe('checkPasswordRules', () => {
  it('detects each rule independently', () => {
    expect(checkPasswordRules('')).toEqual({
      minLength: false,
      uppercase: false,
      lowercase: false,
      digit: false,
    })

    expect(checkPasswordRules('abcdefgh')).toEqual({
      minLength: true,
      uppercase: false,
      lowercase: true,
      digit: false,
    })

    expect(checkPasswordRules('Abcdefg1')).toEqual({
      minLength: true,
      uppercase: true,
      lowercase: true,
      digit: true,
    })
  })
})

describe('isStrongPassword', () => {
  it('accepts passwords meeting all rules', () => {
    expect(isStrongPassword('Password1')).toBe(true)
    expect(isStrongPassword('Aa1aaaaa')).toBe(true)
  })

  it('rejects passwords missing any rule', () => {
    expect(isStrongPassword('short1A')).toBe(false)
    expect(isStrongPassword('alllowercase1')).toBe(false)
    expect(isStrongPassword('ALLUPPERCASE1')).toBe(false)
    expect(isStrongPassword('NoDigitsHere')).toBe(false)
  })

  it('exports a user-facing message constant', () => {
    expect(STRONG_PASSWORD_MESSAGE).toContain('8 karakter')
  })
})
