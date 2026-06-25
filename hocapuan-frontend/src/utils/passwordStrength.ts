export interface PasswordRuleChecks {
  minLength: boolean
  uppercase: boolean
  lowercase: boolean
  digit: boolean
}

export function checkPasswordRules(password: string): PasswordRuleChecks {
  return {
    minLength: password.length >= 8,
    uppercase: /[A-Z]/.test(password),
    lowercase: /[a-z]/.test(password),
    digit: /\d/.test(password),
  }
}

export function isStrongPassword(password: string): boolean {
  const rules = checkPasswordRules(password)
  return rules.minLength && rules.uppercase && rules.lowercase && rules.digit
}

export const STRONG_PASSWORD_MESSAGE =
  'Şifre en az 8 karakter olmalı, en az bir büyük harf, bir küçük harf ve bir sayı içermelidir.'
