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
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      token: null,
      isLoggedIn: false,
      isAdmin: false,
      login: (user, token) => {
        localStorage.setItem('token', token)
        set({
          user,
          token,
          isLoggedIn: true,
          isAdmin: user.role === 'Admin' || user.role === 'Moderator',
        })
      },
      logout: () => {
        localStorage.removeItem('token')
        set({ user: null, token: null, isLoggedIn: false, isAdmin: false })
      },
    }),
    { name: 'hocapuan-auth' }
  )
)
