import { Link, useNavigate } from 'react-router-dom'
import { GraduationCap, Search, LogOut, User } from 'lucide-react'
import { useAuthStore } from '../../store/authStore'
import { useState } from 'react'

export default function Navbar() {
  const { isLoggedIn, user, logout } = useAuthStore()
  const navigate = useNavigate()
  const [query, setQuery] = useState('')

  function handleSearch(e: React.FormEvent) {
    e.preventDefault()
    if (query.trim()) navigate(`/search?q=${encodeURIComponent(query.trim())}`)
  }

  return (
    <header className="bg-white border-b border-surface-border sticky top-0 z-50">
      <div className="max-w-6xl mx-auto px-4 h-16 flex items-center gap-4">

        {/* Logo */}
        <Link to="/" className="flex items-center gap-2 shrink-0">
          <div className="w-8 h-8 rounded-lg bg-primary flex items-center justify-center">
            <GraduationCap className="w-5 h-5 text-white" />
          </div>
          <span className="font-display text-xl text-text hidden sm:block">HocaPuan</span>
        </Link>

        {/* Arama */}
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

        {/* Sağ menü */}
        <nav className="flex items-center gap-2 ml-auto">
          <Link to="/search" className="btn-ghost hidden md:flex">
            Hocalar
          </Link>

          {isLoggedIn ? (
            <div className="flex items-center gap-2">
              <span className="text-sm text-text-muted hidden md:block">
                {user?.username}
              </span>
              <button
                onClick={() => { logout(); navigate('/') }}
                className="btn-ghost"
                title="Çıkış yap"
              >
                <LogOut className="w-4 h-4" />
              </button>
            </div>
          ) : (
            <>
              <Link to="/login"    className="btn-ghost hidden sm:flex">Giriş yap</Link>
              <Link to="/register" className="btn-primary">Kayıt ol</Link>
            </>
          )}
        </nav>
      </div>
    </header>
  )
}
