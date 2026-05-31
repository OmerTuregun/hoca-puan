import { Link, useNavigate } from 'react-router-dom'
import { GraduationCap, Search, LogOut, User, ChevronDown } from 'lucide-react'
import { useAuthStore } from '../../store/authStore'
import { useState, useRef, useEffect } from 'react'

export default function Navbar() {
  const { isLoggedIn, user, logout, hasHydrated } = useAuthStore()
  const navigate = useNavigate()
  const [query, setQuery] = useState('')
  const [menuOpen, setMenuOpen] = useState(false)
  const menuRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        setMenuOpen(false)
      }
    }
    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [])

  function handleSearch(e: React.FormEvent) {
    e.preventDefault()
    if (query.trim()) navigate(`/search?q=${encodeURIComponent(query.trim())}`)
  }

  function handleLogout() {
    setMenuOpen(false)
    logout()
    navigate('/')
  }

  return (
    <header className="bg-white border-b border-surface-border sticky top-0 z-50">
      <div className="max-w-6xl mx-auto px-4 h-16 flex items-center gap-4">

        <Link to="/" className="flex items-center gap-2 shrink-0">
          <div className="w-8 h-8 rounded-lg bg-primary flex items-center justify-center">
            <GraduationCap className="w-5 h-5 text-white" />
          </div>
          <span className="font-display text-xl text-text hidden sm:block">HocaPuan</span>
        </Link>

        <form onSubmit={handleSearch} className="flex-1 max-w-md">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-text-light" />
            <input
              type="text"
              placeholder="Hoca veya üniversite ara..."
              value={query}
              onChange={e => setQuery(e.target.value)}
              className="input pl-9 py-2 text-sm"
            />
          </div>
        </form>

        <nav className="flex items-center gap-2 ml-auto">
          <Link to="/search" className="btn-ghost hidden md:flex">
            Hocalar
          </Link>

          {!hasHydrated ? (
            <span className="text-sm text-text-muted hidden md:block">...</span>
          ) : isLoggedIn ? (
            <div className="relative" ref={menuRef}>
              <button
                type="button"
                onClick={() => setMenuOpen(o => !o)}
                className="btn-ghost flex items-center gap-1.5 max-w-[180px]"
                aria-expanded={menuOpen}
                aria-haspopup="menu"
              >
                <User className="w-4 h-4 shrink-0" />
                <span className="text-sm truncate hidden sm:inline">{user?.username}</span>
                <ChevronDown className={`w-4 h-4 shrink-0 transition-transform ${menuOpen ? 'rotate-180' : ''}`} />
              </button>

              {menuOpen && (
                <div
                  className="absolute right-0 top-full mt-1 w-44 bg-white border border-surface-border rounded-lg shadow-lg py-1 z-50"
                  role="menu"
                >
                  <Link
                    to="/profile"
                    role="menuitem"
                    onClick={() => setMenuOpen(false)}
                    className="flex items-center gap-2 px-4 py-2.5 text-sm text-text hover:bg-surface-alt transition-colors"
                  >
                    <User className="w-4 h-4" />
                    Profilim
                  </Link>
                  <button
                    type="button"
                    role="menuitem"
                    onClick={handleLogout}
                    className="w-full flex items-center gap-2 px-4 py-2.5 text-sm text-text hover:bg-surface-alt transition-colors text-left"
                  >
                    <LogOut className="w-4 h-4" />
                    Çıkış
                  </button>
                </div>
              )}
            </div>
          ) : (
            <>
              <Link to="/login" className="btn-ghost hidden sm:flex">Giriş yap</Link>
              <Link to="/register" className="btn-primary">Kayıt ol</Link>
            </>
          )}
        </nav>
      </div>
    </header>
  )
}
