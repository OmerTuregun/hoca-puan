import { useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { Search } from 'lucide-react'
import { universityApi } from '../services/api'
import Spinner from '../components/ui/Spinner'

export default function UniversitiesPage() {
  const navigate = useNavigate()
  const [q, setQ] = useState('')

  const { data: universities, isLoading } = useQuery({
    queryKey: ['universities'],
    queryFn:  () => universityApi.list(),
  })

  const filtered = useMemo(() => {
    const list = universities ?? []
    const needle = q.trim().toLowerCase()
    if (!needle) return list
    return list.filter(u =>
      (u.name ?? '').toLowerCase().includes(needle)
      || (u.shortName ?? '').toLowerCase().includes(needle)
      || (u.city ?? '').toLowerCase().includes(needle)
    )
  }, [universities, q])

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

