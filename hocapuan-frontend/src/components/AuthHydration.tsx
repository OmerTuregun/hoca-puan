import { useEffect } from 'react'
import { authApi } from '../services/api'
import { profileToUser, useAuthStore } from '../store/authStore'

/** Sayfa yüklendiğinde CSRF token alır ve cookie oturumunu /auth/me ile doğrular. */
export default function AuthHydration() {
  useEffect(() => {
    let cancelled = false

    async function hydrate() {
      const { setCsrfToken, login, logout, finishHydration } = useAuthStore.getState()

      try {
        const { token } = await authApi.getCsrfToken()
        if (!cancelled) setCsrfToken(token)
      } catch {
        if (!cancelled) setCsrfToken(null)
      }

      try {
        const profile = await authApi.me()
        if (!cancelled) login(profileToUser(profile))
      } catch {
        if (!cancelled) logout()
      } finally {
        if (!cancelled) finishHydration()
      }
    }

    void hydrate()
    return () => { cancelled = true }
  }, [])

  return null
}
