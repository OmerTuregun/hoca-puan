import { useCallback, useEffect, useId, useLayoutEffect, useRef, useState } from 'react'
import { createPortal } from 'react-dom'
import { useNavigate } from 'react-router-dom'
import { Search, User, GraduationCap, BookOpen } from 'lucide-react'
import clsx from 'clsx'
import { searchApi, type SearchSuggestion } from '../../services/api'
import { useDebounce } from '../../hooks/useDebounce'

const CATEGORY_LABELS: Record<SearchSuggestion['type'], string> = {
  university: 'Üniversiteler',
  professor: 'Hocalar',
  department: 'Bölümler',
}

const CATEGORY_ICONS = {
  university: GraduationCap,
  professor: User,
  department: BookOpen,
} as const

interface NavbarSearchProps {
  onNavigate?: () => void
}

export default function NavbarSearch({ onNavigate }: NavbarSearchProps) {
  const navigate = useNavigate()
  const listboxId = useId()
  const containerRef = useRef<HTMLDivElement>(null)
  const dropdownRef = useRef<HTMLDivElement>(null)
  const inputRef = useRef<HTMLInputElement>(null)

  const [query, setQuery] = useState('')
  const [open, setOpen] = useState(false)
  const [loading, setLoading] = useState(false)
  const [suggestions, setSuggestions] = useState<SearchSuggestion[]>([])
  const [activeIndex, setActiveIndex] = useState(-1)
  const [dropdownRect, setDropdownRect] = useState<{ top: number; left: number; width: number } | null>(null)

  const debouncedQuery = useDebounce(query, 350)
  const trimmed = debouncedQuery.trim()
  const shouldFetch = trimmed.length >= 2

  const updateDropdownPosition = useCallback(() => {
    const el = inputRef.current
    if (!el) return
    const rect = el.getBoundingClientRect()
    setDropdownRect({
      top: rect.bottom + 4,
      left: rect.left,
      width: rect.width,
    })
  }, [])

  useEffect(() => {
    if (!shouldFetch) {
      setSuggestions([])
      setLoading(false)
      setActiveIndex(-1)
      return
    }

    let cancelled = false
    setLoading(true)

    searchApi.suggestions(trimmed)
      .then(items => {
        if (!cancelled) {
          setSuggestions(items)
          setActiveIndex(-1)
        }
      })
      .catch(() => {
        if (!cancelled) setSuggestions([])
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })

    return () => { cancelled = true }
  }, [trimmed, shouldFetch])

  useLayoutEffect(() => {
    if (!open || !shouldFetch) {
      setDropdownRect(null)
      return
    }

    updateDropdownPosition()

    window.addEventListener('resize', updateDropdownPosition)
    window.addEventListener('scroll', updateDropdownPosition, true)

    return () => {
      window.removeEventListener('resize', updateDropdownPosition)
      window.removeEventListener('scroll', updateDropdownPosition, true)
    }
  }, [open, shouldFetch, suggestions.length, loading, updateDropdownPosition])

  useEffect(() => {
    if (!open) return

    function handleClickOutside(e: MouseEvent) {
      const target = e.target as Node
      if (containerRef.current?.contains(target)) return
      if (dropdownRef.current?.contains(target)) return
      setOpen(false)
      setActiveIndex(-1)
    }

    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [open])

  const flatSuggestions = suggestions

  const selectSuggestion = useCallback((item: SearchSuggestion) => {
    setOpen(false)
    setActiveIndex(-1)
    onNavigate?.()

    if (item.type === 'professor') {
      navigate(`/professors/${item.id}`)
    } else if (item.type === 'university') {
      navigate(`/universities/${item.id}`)
    } else if (item.type === 'department' && item.universityId) {
      navigate(`/universities/${item.universityId}/departments/${item.id}`)
    }
  }, [navigate, onNavigate])

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (activeIndex >= 0 && flatSuggestions[activeIndex]) {
      selectSuggestion(flatSuggestions[activeIndex])
      return
    }
    if (query.trim()) {
      navigate(`/search?q=${encodeURIComponent(query.trim())}`)
      setOpen(false)
      onNavigate?.()
    }
  }

  function handleKeyDown(e: React.KeyboardEvent<HTMLInputElement>) {
    if (e.key === 'Escape') {
      setOpen(false)
      setActiveIndex(-1)
      return
    }

    if (!open || flatSuggestions.length === 0) return

    if (e.key === 'ArrowDown') {
      e.preventDefault()
      setActiveIndex(i => (i + 1) % flatSuggestions.length)
    } else if (e.key === 'ArrowUp') {
      e.preventDefault()
      setActiveIndex(i => (i <= 0 ? flatSuggestions.length - 1 : i - 1))
    } else if (e.key === 'Enter' && activeIndex >= 0) {
      e.preventDefault()
      selectSuggestion(flatSuggestions[activeIndex])
    }
  }

  const showDropdown = open && shouldFetch && dropdownRect !== null
  const showEmpty = showDropdown && !loading && flatSuggestions.length === 0
  const grouped = (['university', 'professor', 'department'] as const)
    .map(type => ({
      type,
      items: flatSuggestions.filter(s => s.type === type),
    }))
    .filter(g => g.items.length > 0)

  let runningIndex = -1

  const dropdown = showDropdown && (
    <div
      ref={dropdownRef}
      id={listboxId}
      role="listbox"
      style={{
        position: 'fixed',
        top: dropdownRect.top,
        left: dropdownRect.left,
        width: dropdownRect.width,
      }}
      className="z-[100] max-h-[min(70vh,24rem)] overflow-y-auto overflow-x-hidden rounded-lg border border-surface-border bg-white shadow-lg"
    >
      {loading && (
        <div className="flex items-center justify-center py-6">
          <div className="w-5 h-5 border-2 border-surface-border border-t-primary rounded-full animate-spin" />
        </div>
      )}

      {showEmpty && (
        <p className="px-4 py-3 text-sm text-text-muted">Sonuç bulunamadı</p>
      )}

      {!loading && grouped.map(group => {
        const Icon = CATEGORY_ICONS[group.type]
        return (
          <div key={group.type}>
            <div className="px-3 py-1.5 text-xs font-semibold uppercase tracking-wide text-text-muted bg-surface-alt sticky top-0">
              {CATEGORY_LABELS[group.type]}
            </div>
            <ul>
              {group.items.map(item => {
                runningIndex += 1
                const itemIndex = runningIndex
                const isActive = itemIndex === activeIndex
                return (
                  <li key={`${item.type}-${item.id}`} role="presentation">
                    <button
                      type="button"
                      id={`${listboxId}-option-${itemIndex}`}
                      role="option"
                      aria-selected={isActive}
                      onMouseDown={e => e.preventDefault()}
                      onClick={() => selectSuggestion(item)}
                      onMouseEnter={() => setActiveIndex(itemIndex)}
                      className={clsx(
                        'flex w-full items-start gap-3 px-3 py-2.5 text-left transition-colors',
                        isActive ? 'bg-primary/10' : 'hover:bg-surface-alt',
                      )}
                    >
                      <Icon className="mt-0.5 h-4 w-4 shrink-0 text-text-muted" />
                      <span className="min-w-0 flex-1">
                        <span className="block truncate text-sm font-medium text-text">
                          {item.name}
                        </span>
                        {item.context && (
                          <span className="block truncate text-xs text-text-muted">
                            {item.context}
                          </span>
                        )}
                      </span>
                    </button>
                  </li>
                )
              })}
            </ul>
          </div>
        )
      })}
    </div>
  )

  return (
    <form onSubmit={handleSubmit} className="flex-1 max-w-md min-w-0">
      <div className="relative" ref={containerRef}>
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-text-light pointer-events-none z-10" />
        <input
          ref={inputRef}
          type="text"
          role="combobox"
          aria-expanded={showDropdown}
          aria-controls={showDropdown ? listboxId : undefined}
          aria-autocomplete="list"
          aria-activedescendant={
            activeIndex >= 0 ? `${listboxId}-option-${activeIndex}` : undefined
          }
          placeholder="Hoca veya üniversite ara..."
          value={query}
          onChange={e => {
            setQuery(e.target.value)
            setOpen(true)
          }}
          onFocus={() => {
            if (shouldFetch) setOpen(true)
          }}
          onKeyDown={handleKeyDown}
          className="input pl-9 py-2 text-sm w-full"
          autoComplete="off"
        />

        {dropdown && createPortal(dropdown, document.body)}
      </div>
    </form>
  )
}
