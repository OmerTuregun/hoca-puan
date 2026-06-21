import { authApi } from '../services/api'
import { useAuthStore } from '../store/authStore'

/** State-değiştiren istek öncesi CSRF token'ın hazır olduğundan emin olur. */
export async function ensureCsrfToken(): Promise<string> {
  const existing = useAuthStore.getState().csrfToken
  if (existing) return existing

  const { token } = await authApi.getCsrfToken()
  useAuthStore.getState().setCsrfToken(token)
  return token
}
