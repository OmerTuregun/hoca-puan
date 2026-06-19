import { create } from 'zustand'
import { persist } from 'zustand/middleware'

interface User {
  id: number
  username: string
  email: string
  role: string
  universityName?: string
  isEmailVerified: boolean
}

interface AuthState {
  user: User | null
  token: string | null
  login: (user: User, token: string) => void
  logout: () => void
  isLoggedIn: boolean
  isAdmin: boolean
  /** persist rehydrate bitene kadar false — ilk render'da yanlış "girişsiz" UI önlenir */
  hasHydrated: boolean
  finishHydration: () => void
}

function computeIsAdmin(user: User | null) {
  return user?.role === 'Admin' || user?.role === 'Moderator'
}

let finishHydrationRef: (() => void) | null = null

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => {
      const state: AuthState = {
      user: null,
      token: null,
      isLoggedIn: false,
      isAdmin: false,
      hasHydrated: false,

      finishHydration: () => {
        const { user, token } = get()
        const storedToken = token || localStorage.getItem('token')

        if (user && storedToken) {
          localStorage.setItem('token', storedToken)
          set({
            token: storedToken,
            isLoggedIn: true,
            isAdmin: computeIsAdmin(user),
            hasHydrated: true,
          })
          return
        }

        localStorage.removeItem('token')
        set({
          user: null,
          token: null,
          isLoggedIn: false,
          isAdmin: false,
          hasHydrated: true,
        })
      },

      login: (user, token) => {
        localStorage.setItem('token', token)
        set({
          user,
          token,
          isLoggedIn: true,
          isAdmin: computeIsAdmin(user),
          hasHydrated: true,
        })
      },

      logout: () => {
        localStorage.removeItem('token')
        set({
          user: null,
          token: null,
          isLoggedIn: false,
          isAdmin: false,
          hasHydrated: true,
        })
      },
    }

      finishHydrationRef = state.finishHydration
      return state
    },
    {
      name: 'hocapuan-auth',
      partialize: state => ({
        user: state.user,
        token: state.token,
      }),
      onRehydrateStorage: () => (_state, error) => {
        if (error) {
          console.error('Auth persist rehydrate failed:', error)
        }
        finishHydrationRef?.()
      },
    }
  )
)
