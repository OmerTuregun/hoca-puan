import { Link, useNavigate } from 'react-router-dom'
import {
  GraduationCap,
  ChevronDown,
  Menu,
  X,
  User,
} from 'lucide-react'
import { useAuthStore } from '../../store/authStore'
import { authApi } from '../../services/api'
import NavbarSearch from './NavbarSearch'
import {
  getDesktopDropdownNavItems,
  getDesktopInlineNavItems,
  getVisibleNavItems,
  type NavItem,
} from '../../constants/navItems'
import { useState, useRef, useEffect, useCallback } from 'react'
import { createPortal } from 'react-dom'
import clsx from 'clsx'

export default function Navbar() {
  const { isLoggedIn, user, logout, hasHydrated, isAdmin } = useAuthStore()
  const navigate = useNavigate()
  const [userMenuOpen, setUserMenuOpen] = useState(false)
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false)
  const userMenuRef = useRef<HTMLDivElement>(null)
  const mobileMenuRef = useRef<HTMLDivElement>(null)

  const navContext = { hasHydrated, isLoggedIn, isAdmin }
  const desktopInlineItems = getDesktopInlineNavItems(navContext)
  const desktopDropdownItems = getDesktopDropdownNavItems(navContext)
  const mobileNavItems = getVisibleNavItems(navContext)

  const closeMobileMenu = useCallback(() => setMobileMenuOpen(false), [])

  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (userMenuRef.current && !userMenuRef.current.contains(e.target as Node)) {
        setUserMenuOpen(false)
      }
    }
    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [])

  useEffect(() => {
    if (!mobileMenuOpen) return

    function handleKeyDown(e: KeyboardEvent) {
      if (e.key === 'Escape') closeMobileMenu()
    }

    const scrollY = window.scrollY
    const prevOverflow = document.body.style.overflow
    const prevPosition = document.body.style.position
    const prevTop = document.body.style.top
    const prevWidth = document.body.style.width

    document.addEventListener('keydown', handleKeyDown)
    document.body.style.overflow = 'hidden'
    document.body.style.position = 'fixed'
    document.body.style.top = `-${scrollY}px`
    document.body.style.width = '100%'

    return () => {
      document.removeEventListener('keydown', handleKeyDown)
      document.body.style.overflow = prevOverflow
      document.body.style.position = prevPosition
      document.body.style.top = prevTop
      document.body.style.width = prevWidth
      window.scrollTo(0, scrollY)
    }
  }, [mobileMenuOpen, closeMobileMenu])

  function handleSearchNavigate() {
    closeMobileMenu()
  }

  async function handleLogout() {
    setUserMenuOpen(false)
    closeMobileMenu()
    try {
      await authApi.logout()
    } catch {
      // Sunucu hatası olsa bile yerel oturumu kapat
    }
    logout()
    navigate('/')
  }

  function handleNavAction(item: NavItem) {
    if (item.action === 'logout') {
      void handleLogout()
      return
    }
    closeMobileMenu()
  }

  function renderMobileNavItem(item: NavItem) {
    const Icon = item.icon
    const isPrimary = item.variant === 'primary'
    const className = clsx(
      'flex w-full items-center gap-3 text-lg font-semibold transition-colors min-h-[52px]',
      isPrimary
        ? 'mt-8 justify-center rounded-xl bg-primary px-5 py-4 text-white hover:bg-primary-hover active:scale-[0.99]'
        : 'border-b border-surface-border px-2 py-4 text-text hover:bg-surface-alt active:bg-surface-border',
    )

    if (item.to) {
      return (
        <Link
          key={item.id}
          to={item.to}
          onClick={() => closeMobileMenu()}
          className={className}
        >
          {Icon && <Icon className="h-6 w-6 shrink-0" />}
          {item.label}
        </Link>
      )
    }

    return (
      <button
        key={item.id}
        type="button"
        onClick={() => handleNavAction(item)}
        className={clsx(className, 'text-left')}
      >
        {Icon && <Icon className="h-6 w-6 shrink-0" />}
        {item.label}
      </button>
    )
  }

  return (
    <header className="bg-white border-b border-surface-border sticky top-0 z-50">
      <div className="max-w-6xl mx-auto px-4 h-16 flex items-center gap-4">

        <Link to="/" className="flex items-center gap-2 shrink-0 min-h-[44px]">
          <div className="w-8 h-8 rounded-lg bg-primary flex items-center justify-center">
            <GraduationCap className="w-5 h-5 text-white" />
          </div>
          <span className="font-display text-xl text-text hidden sm:block">Hocanı Yorumla</span>
        </Link>

        <NavbarSearch onNavigate={handleSearchNavigate} />

        {/* Desktop nav — mevcut davranış korunur */}
        <nav className="hidden md:flex items-center gap-2 ml-auto shrink-0">
          {desktopInlineItems.map(item =>
            item.to ? (
              <Link
                key={item.id}
                to={item.to}
                className={item.desktopClassName}
              >
                {item.label}
              </Link>
            ) : null,
          )}

          {!hasHydrated ? (
            <span className="text-sm text-text-muted">...</span>
          ) : isLoggedIn ? (
            <div className="relative" ref={userMenuRef}>
              <button
                type="button"
                onClick={() => setUserMenuOpen(o => !o)}
                className="btn-ghost flex items-center gap-1.5 max-w-[180px]"
                aria-expanded={userMenuOpen}
                aria-haspopup="menu"
              >
                <User className="w-4 h-4 shrink-0" />
                <span className="text-sm truncate hidden sm:inline">{user?.username}</span>
                <ChevronDown className={`w-4 h-4 shrink-0 transition-transform ${userMenuOpen ? 'rotate-180' : ''}`} />
              </button>

              {userMenuOpen && (
                <div
                  className="absolute right-0 top-full mt-1 w-44 bg-white border border-surface-border rounded-lg shadow-lg py-1 z-50"
                  role="menu"
                >
                  {desktopDropdownItems.map(item => {
                    const Icon = item.icon
                    if (item.to) {
                      return (
                        <Link
                          key={item.id}
                          to={item.to}
                          role="menuitem"
                          onClick={() => setUserMenuOpen(false)}
                          className="flex items-center gap-2 px-4 py-2.5 text-sm text-text hover:bg-surface-alt transition-colors"
                        >
                          {Icon && <Icon className="w-4 h-4" />}
                          {item.label}
                        </Link>
                      )
                    }
                    return (
                      <button
                        key={item.id}
                        type="button"
                        role="menuitem"
                        onClick={() => void handleLogout()}
                        className="w-full flex items-center gap-2 px-4 py-2.5 text-sm text-text hover:bg-surface-alt transition-colors text-left"
                      >
                        {Icon && <Icon className="w-4 h-4" />}
                        {item.label}
                      </button>
                    )
                  })}
                </div>
              )}
            </div>
          ) : null}
        </nav>

        {/* Mobil hamburger */}
        <button
          type="button"
          className="md:hidden ml-auto shrink-0 btn-ghost min-h-[44px] min-w-[44px] p-2"
          aria-label="Menüyü aç"
          aria-expanded={mobileMenuOpen}
          onClick={() => setMobileMenuOpen(true)}
        >
          <Menu className="w-6 h-6" />
        </button>
      </div>

      {mobileMenuOpen && createPortal(
        <div
          className="fixed inset-0 z-[9999] md:hidden"
          ref={mobileMenuRef}
          role="dialog"
          aria-modal="true"
          aria-label="Mobil navigasyon"
        >
          <button
            type="button"
            className="absolute inset-0 bg-black/50"
            aria-label="Menü arka planını kapat"
            onClick={closeMobileMenu}
          />
          <aside
            className="absolute inset-0 z-10 flex flex-col bg-white animate-slideInRight"
            aria-label="Mobil menü"
          >
            <div className="flex h-16 shrink-0 items-center justify-between border-b border-surface-border px-5">
              <span className="font-display text-xl text-text">Menü</span>
              <button
                type="button"
                onClick={closeMobileMenu}
                className="flex h-12 w-12 items-center justify-center rounded-lg text-text transition-colors hover:bg-surface-alt"
                aria-label="Menüyü kapat"
              >
                <X className="h-7 w-7" strokeWidth={2.5} />
              </button>
            </div>

            <nav className="flex flex-1 flex-col overflow-y-auto px-6 py-8">
              {!hasHydrated ? (
                <p className="px-2 py-4 text-base text-text-muted">Yükleniyor...</p>
              ) : (
                <>
                  <div className="flex flex-col">
                    {mobileNavItems
                      .filter(item => item.variant !== 'primary')
                      .map(renderMobileNavItem)}
                  </div>
                  {mobileNavItems
                    .filter(item => item.variant === 'primary')
                    .map(renderMobileNavItem)}
                </>
              )}
            </nav>
          </aside>
        </div>,
        document.body,
      )}
    </header>
  )
}
