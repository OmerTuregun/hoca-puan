import { useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { Search } from 'lucide-react'
import { universityApi } from '../services/api'
import { useDebounce } from '../hooks/useDebounce'
import Spinner from '../components/ui/Spinner'

function normalizeSearch(s: string) {
  return s.trim().toLocaleLowerCase('tr-TR')
}

export default function UniversitiesPage() {
  const navigate = useNavigate()
  const [q, setQ] = useState('')
  const debouncedQ = useDebounce(q, 300)

  const { data: universities, isLoading, isFetching } = useQuery({
    queryKey: ['universities', debouncedQ.trim()],
    queryFn: () => universityApi.list(debouncedQ.trim() || undefined),
  })

  const filtered = useMemo(() => {
    const list = universities ?? []
    const needle = normalizeSearch(debouncedQ)
    if (!needle) return list
    return list.filter(u =>
      normalizeSearch(u.name ?? '').includes(needle)
      || normalizeSearch(u.shortName ?? '').includes(needle)
      || normalizeSearch(u.city ?? '').includes(needle)
      || normalizeSearch(u.name ?? '').replace(/\s+/g, '').includes(needle.replace(/\s+/g, ''))
    )
  }, [universities, debouncedQ])

  return (
    <div className="max-w-6xl mx-auto px-4 py-8">
      <div className="flex flex-col sm:flex-row sm:items-end gap-3 mb-6">
        <div className="flex-1">
          <h1 className="font-display text-2xl text-text">Üniversiteler</h1>
          <p className="text-sm text-text-muted mt-1">
            {filtered.length} üniversite
          </p>
        </div>

        <div className="w-full sm:w-80">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-text-light" />
            <input
              value={q}
              onChange={e => setQ(e.target.value)}
              placeholder="Üniversite adı, şehir..."
              className="input pl-9 py-2 text-sm"
            />
          </div>
        </div>
      </div>

      {isLoading ? (
        <Spinner />
      ) : (
        <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-3">
          {filtered.length === 0 && !isFetching ? (
            <p className="text-sm text-text-muted col-span-full py-8 text-center">
              Aramanızla eşleşen üniversite bulunamadı.
            </p>
          ) : null}
          {filtered.map(u => (
            <button
              key={u.id}
              onClick={() => navigate(`/universities/${u.id}`)}
              className="card-hover p-4 text-left"
            >
              <div className="flex items-center justify-between gap-3">
                <div className="min-w-0">
                  <p className="font-semibold text-text text-sm leading-tight truncate">{u.name}</p>
                  <p className="text-xs text-text-muted mt-0.5">{u.city} · {u.type}</p>
                </div>
                <span className="badge-primary shrink-0">{u.totalProfessors} hoca</span>
              </div>
            </button>
          ))}
        </div>
      )}
    </div>
  )
}

