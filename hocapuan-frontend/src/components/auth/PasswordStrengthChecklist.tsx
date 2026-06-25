import { Check, X } from 'lucide-react'
import clsx from 'clsx'
import { checkPasswordRules } from '../../utils/passwordStrength'

interface PasswordStrengthChecklistProps {
  password: string
}

const RULES = [
  { key: 'minLength' as const, label: 'En az 8 karakter' },
  { key: 'uppercase' as const, label: 'En az bir büyük harf (A-Z)' },
  { key: 'lowercase' as const, label: 'En az bir küçük harf (a-z)' },
  { key: 'digit' as const, label: 'En az bir sayı' },
]

export default function PasswordStrengthChecklist({ password }: PasswordStrengthChecklistProps) {
  if (!password) return null

  const checks = checkPasswordRules(password)
  const allMet = RULES.every(rule => checks[rule.key])

  return (
    <div className="mt-2" aria-live="polite">
      <ul className="space-y-1">
        {RULES.map(rule => {
          const met = checks[rule.key]
          const Icon = met ? Check : X
          return (
            <li
              key={rule.key}
              className={clsx(
                'flex items-center gap-2 text-xs',
                met ? 'text-green-700' : 'text-danger',
              )}
            >
              <Icon
                className={clsx(
                  'h-3.5 w-3.5 shrink-0',
                  met ? 'text-green-600' : 'text-danger',
                )}
                aria-hidden
              />
              {rule.label}
            </li>
          )
        })}
      </ul>
      {!allMet && (
        <p className="text-xs text-danger mt-2">
          Kayıt olabilmek için yukarıdaki tüm şifre kurallarını sağlamalısınız.
        </p>
      )}
    </div>
  )
}
