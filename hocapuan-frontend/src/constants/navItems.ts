import type { LucideIcon } from 'lucide-react'
import { LogOut, ShieldAlert, User } from 'lucide-react'

export type NavAudience = 'guest' | 'user' | 'admin'

export interface NavContext {
  hasHydrated: boolean
  isLoggedIn: boolean
  isAdmin: boolean
}

export interface NavItem {
  id: string
  label: string
  to?: string
  action?: 'logout'
  icon?: LucideIcon
  variant?: 'ghost' | 'primary'
  /** Kimler görebilir — boş = herkes (oturum yüklenince) */
  audiences: NavAudience[]
  /** Desktop'ta nav çubuğunda doğrudan link olarak göster */
  desktopInline?: boolean
  /** Desktop inline link için Tailwind sınıfları (mevcut davranışı korumak için) */
  desktopClassName?: string
}

const GUEST: NavAudience[] = ['guest']
const USER: NavAudience[] = ['user']
const ADMIN: NavAudience[] = ['admin']

/** Navbar linkleri — yeni link eklemek için yalnızca bu listeyi güncelleyin. */
export const NAV_ITEMS: NavItem[] = [
  {
    id: 'search',
    label: 'Hocalar',
    to: '/search',
    audiences: ['guest', 'user', 'admin'],
    desktopInline: true,
    desktopClassName: 'btn-ghost hidden md:flex',
  },
  {
    id: 'login',
    label: 'Giriş yap',
    to: '/login',
    audiences: GUEST,
    variant: 'ghost',
    desktopInline: true,
    desktopClassName: 'btn-ghost hidden sm:flex',
  },
  {
    id: 'register',
    label: 'Kayıt ol',
    to: '/register',
    audiences: GUEST,
    variant: 'primary',
    desktopInline: true,
    desktopClassName: 'btn-primary',
  },
  {
    id: 'profile',
    label: 'Profilim',
    to: '/profile',
    icon: User,
    audiences: USER,
  },
  {
    id: 'moderation',
    label: 'Moderasyon',
    to: '/admin/moderation',
    icon: ShieldAlert,
    audiences: ADMIN,
  },
  {
    id: 'logout',
    label: 'Çıkış',
    action: 'logout',
    icon: LogOut,
    audiences: USER,
  },
]

function activeAudiences(ctx: NavContext): NavAudience[] {
  if (!ctx.hasHydrated) return []
  if (!ctx.isLoggedIn) return ['guest']
  const audiences: NavAudience[] = ['user']
  if (ctx.isAdmin) audiences.push('admin')
  return audiences
}

export function getVisibleNavItems(ctx: NavContext): NavItem[] {
  const active = activeAudiences(ctx)
  if (active.length === 0) return []
  return NAV_ITEMS.filter(item =>
    item.audiences.some(a => active.includes(a)),
  )
}

export function getDesktopInlineNavItems(ctx: NavContext): NavItem[] {
  return getVisibleNavItems(ctx).filter(item => item.desktopInline)
}

export function getDesktopDropdownNavItems(ctx: NavContext): NavItem[] {
  return getVisibleNavItems(ctx).filter(item => !item.desktopInline)
}
