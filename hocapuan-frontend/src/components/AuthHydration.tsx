import { useEffect } from 'react'
import { useAuthStore } from '../store/authStore'

/** Sayfa yenilendiğinde Zustand persist + token senkronunu garanti eder. */
export default function AuthHydration() {
  useEffect(() => {
    const finish = () => useAuthStore.getState().finishHydration()

    if (useAuthStore.persist.hasHydrated()) {
      finish()
      return
    }

    const unsub = useAuthStore.persist.onFinishHydration(finish)
    void useAuthStore.persist.rehydrate()

    return unsub
  }, [])

  return null
}
