import { create } from 'zustand'

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
  csrfToken: string | null
  login: (user: User) => void
  logout: () => void
  setCsrfToken: (token: string | null) => void
  isLoggedIn: boolean
  isAdmin: boolean
  hasHydrated: boolean
  finishHydration: () => void
}

function computeIsAdmin(user: User | null) {
  return user?.role === 'Admin' || user?.role === 'Moderator'
}

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  csrfToken: null,
  isLoggedIn: false,
  isAdmin: false,
  hasHydrated: false,

  finishHydration: () => set({ hasHydrated: true }),

  login: (user) => {
    set({
      user,
      isLoggedIn: true,
      isAdmin: computeIsAdmin(user),
      hasHydrated: true,
    })
  },

  logout: () => {
    set({
      user: null,
      csrfToken: null,
      isLoggedIn: false,
      isAdmin: false,
      hasHydrated: true,
    })
  },

  setCsrfToken: (token) => set({ csrfToken: token }),
}))

export function profileToUser(profile: {
  id: number
  username: string
  email: string
  role: string
  universityName?: string
  isEmailVerified: boolean
}): User {
  return {
    id: profile.id,
    username: profile.username,
    email: profile.email,
    role: profile.role,
    universityName: profile.universityName,
    isEmailVerified: profile.isEmailVerified,
  }
}
