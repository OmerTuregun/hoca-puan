import { useState, useEffect } from 'react'
import { useSearchParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { SlidersHorizontal, ChevronLeft, ChevronRight } from 'lucide-react'
import { professorApi, universityApi } from '../services/api'
import ProfessorCard from '../components/professor/ProfessorCard'
import Spinner from '../components/ui/Spinner'

const SORT_OPTIONS = [
  { value: 'quality',    label: 'En yüksek puan' },
  { value: 'reviews',   label: 'En çok yorum' },
  { value: 'difficulty', label: 'En zor' },
]

export default function SearchPage() {
  const [params, setParams] = useSearchParams()
  const [page, setPage] = useState(1)

  const query        = params.get('q') || ''
  const universityId = params.get('universityId') ? Number(params.get('universityId')) : undefined
  const facultyId    = params.get('facultyId') ? Number(params.get('facultyId')) : undefined
  const sortBy       = params.get('sort') || 'quality'

  // URL değişince sayfayı sıfırla
  useEffect(() => { setPage(1) }, [query, universityId, facultyId, sortBy])

  const { data: universities } = useQuery({
    queryKey: ['universities'],
    queryFn:  () => universityApi.list(),
  })

  const { data: faculties } = useQuery({
    queryKey: ['faculties', universityId],
    queryFn:  () => universityApi.faculties(universityId!),
    enabled:  !!universityId,
  })

  const { data, isLoading } = useQuery({
    queryKey: ['professors', query, universityId, facultyId, sortBy, page],
    queryFn:  () => professorApi.search({ query, universityId, facultyId, sortBy, page, pageSize: 12 }),
    placeholderData: prev => prev,
  })

  function setParam(key: string, val: string | undefined) {
    const next = new URLSearchParams(params)
    if (val) next.set(key, val)
    else next.delete(key)
    setParams(next)
  }

  return (
    <div className="max-w-6xl mx-auto px-4 py-8">
      <div className="flex flex-col lg:flex-row gap-6">

        {/* ─── Filtre paneli ─── */}
        <aside className="lg:w-56 shrink-0">
          <div className="card p-4 sticky top-20">
            <h2 className="font-semibold text-sm text-text flex items-center gap-2 mb-4">
              <SlidersHorizontal className="w-4 h-4 text-primary" />
              Filtreler
            </h2>

            {/* Üniversite */}
            <div className="mb-4">
              <label className="text-xs font-medium text-text-muted mb-1.5 block">Üniversite</label>
              <select
                value={universityId ?? ''}
                onChange={e => {
                  const next = new URLSearchParams(params)
                  const val = e.target.value
                  if (val) next.set('universityId', val)
                  else next.delete('universityId')
                  next.delete('facultyId')
                  setParams(next)
                }}
                className="input text-sm py-2"
              >
                <option value="">Tümü</option>
                {universities?.map(u => (
                  <option key={u.id} value={u.id}>{u.name}</option>
                ))}
              </select>
            </div>

            {/* Fakülte */}
            {universityId && (
              <div className="mb-4">
                <label className="text-xs font-medium text-text-muted mb-1.5 block">Fakülte</label>
                <select
                  value={facultyId ?? ''}
                  onChange={e => setParam('facultyId', e.target.value || undefined)}
                  className="input text-sm py-2"
                  disabled={!faculties?.length}
                >
                  <option value="">Tüm Fakülteler</option>
                  {faculties?.map(f => (
                    <option key={f.id} value={f.id}>{f.name}</option>
                  ))}
                </select>
              </div>
            )}

            {/* Sıralama */}
            <div>
              <label className="text-xs font-medium text-text-muted mb-1.5 block">Sırala</label>
              {SORT_OPTIONS.map(opt => (
                <button
                  key={opt.value}
                  onClick={() => setParam('sort', opt.value)}
                  className={`w-full text-left px-3 py-2 rounded text-sm mb-1 transition-colors ${
                    sortBy === opt.value
                      ? 'bg-primary-light text-primary font-medium'
                      : 'hover:bg-surface-alt text-text-muted'
                  }`}
                >
                  {opt.label}
                </button>
              ))}
            </div>

            {(query || universityId || facultyId) && (
              <button
                onClick={() => { setParams({}); setPage(1) }}
                className="mt-4 text-xs text-text-muted hover:text-danger underline"
              >
                Filtreleri temizle
              </button>
            )}
          </div>
        </aside>

        {/* ─── Sonuçlar ─── */}
        <div className="flex-1">
          {/* Başlık */}
          <div className="flex items-center justify-between mb-4">
            <div>
              <h1 className="font-display text-2xl text-text">
                {query ? `"${query}" sonuçları` : 'Tüm hocalar'}
              </h1>
              {data && (
                <p className="text-sm text-text-muted mt-0.5">
                  {data.totalCount} hoca bulundu
                </p>
              )}
            </div>
          </div>

          {/* Arama kutusu (mobil için de) */}
          <input
            type="text"
            defaultValue={query}
            placeholder="Hoca adı, bölüm..."
            className="input mb-4"
            onKeyDown={e => {
              if (e.key === 'Enter') {
                setParam('q', (e.target as HTMLInputElement).value || undefined)
              }
            }}
          />

          {isLoading ? (
            <Spinner />
          ) : data?.items.length === 0 ? (
            <div className="card p-12 text-center">
              <p className="text-text-muted">Sonuç bulunamadı.</p>
              <p className="text-sm text-text-light mt-1">Farklı bir arama terimi deneyin.</p>
            </div>
          ) : (
            <>
              <div className="grid sm:grid-cols-2 gap-3">
                {data?.items.map(p => (
                  <ProfessorCard key={p.id} professor={p} />
                ))}
              </div>

              {/* Sayfalama */}
              {data && data.totalPages > 1 && (
                <div className="flex items-center justify-center gap-2 mt-8">
                  <button
                    onClick={() => setPage(p => Math.max(1, p - 1))}
                    disabled={!data.hasPreviousPage}
                    className="btn-outline px-3 py-2 disabled:opacity-40"
                  >
                    <ChevronLeft className="w-4 h-4" />
                  </button>
                  <span className="text-sm text-text-muted px-4">
                    {page} / {data.totalPages}
                  </span>
                  <button
                    onClick={() => setPage(p => p + 1)}
                    disabled={!data.hasNextPage}
                    className="btn-outline px-3 py-2 disabled:opacity-40"
                  >
                    <ChevronRight className="w-4 h-4" />
                  </button>
                </div>
              )}
            </>
          )}
        </div>
      </div>
    </div>
  )
}
