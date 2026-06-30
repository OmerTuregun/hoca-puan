import { authApi } from '../services/api'
import { useAuthStore } from '../store/authStore'

let inFlight: Promise<string> | null = null

/** State-değiştiren istek öncesi sunucudan güncel CSRF token alır (cookie ile eşleşir). */
export async function ensureCsrfToken(): Promise<string> {
  if (inFlight) return inFlight

  inFlight = authApi
    .getCsrfToken()
    .then(({ token }) => {
      useAuthStore.getState().setCsrfToken(token)
      return token
    })
    .finally(() => {
      inFlight = null
    })

  return inFlight
}
