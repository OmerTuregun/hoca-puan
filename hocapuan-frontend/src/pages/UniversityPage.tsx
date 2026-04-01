import { useParams, useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { Globe, MapPin, Star } from 'lucide-react'
import { universityApi, professorApi } from '../services/api'
import ProfessorCard from '../components/professor/ProfessorCard'
import Spinner from '../components/ui/Spinner'

export default function UniversityPage() {
  const { id } = useParams<{ id: string }>()
  const uniId = Number(id)
  const navigate = useNavigate()

  const { data: university, isLoading } = useQuery({
    queryKey: ['university', uniId],
    queryFn:  () => universityApi.get(uniId),
  })

  const { data: professors } = useQuery({
    queryKey: ['professors-uni', uniId],
    queryFn:  () => professorApi.search({ universityId: uniId, pageSize: 12 }),
    enabled: !!uniId,
  })

  if (isLoading) return <Spinner />
  if (!university) return <div className="text-center py-20">Üniversite bulunamadı.</div>

  const u = university

  return (
    <div className="max-w-5xl mx-auto px-4 py-8">
      {/* Başlık */}
      <div className="card p-6 mb-6 animate-fadeUp">
        <div className="flex items-start gap-4">
          <div className="w-16 h-16 rounded-xl bg-primary-light flex items-center justify-center text-primary font-bold text-xl shrink-0">
            {u.shortName?.substring(0, 3)}
          </div>
          <div className="flex-1">
            <span className={`badge ${u.type === 'Vakıf' ? 'badge-warning' : 'badge-primary'} mb-2`}>
              {u.type}
            </span>
            <h1 className="font-display text-3xl text-text">{u.name}</h1>
            <div className="flex flex-wrap gap-4 mt-2 text-sm text-text-muted">
              <span className="flex items-center gap-1.5">
                <MapPin className="w-4 h-4" />{u.city}
              </span>
              {u.website && (
                <a href={u.website} target="_blank" rel="noopener" className="flex items-center gap-1.5 hover:text-primary transition-colors">
                  <Globe className="w-4 h-4" />Web sitesi
                </a>
              )}
            </div>
          </div>
        </div>

        {/* İstatistikler */}
        <div className="mt-5 pt-5 border-t border-surface-border grid grid-cols-3 gap-4 text-center">
          <div>
            <p className="text-2xl font-bold text-text">{u.totalProfessors}</p>
            <p className="text-xs text-text-muted mt-0.5">Hoca</p>
          </div>
          <div>
            <p className="text-2xl font-bold text-text">{u.totalReviews}</p>
            <p className="text-xs text-text-muted mt-0.5">Yorum</p>
          </div>
          <div>
            <p className="text-2xl font-bold text-primary">{u.averageRating > 0 ? u.averageRating.toFixed(1) : '—'}</p>
            <p className="text-xs text-text-muted mt-0.5">Ort. puan</p>
          </div>
        </div>
      </div>

      {/* Hocalar */}
      <div className="flex items-center justify-between mb-4">
        <h2 className="font-display text-xl text-text">Hocalar</h2>
        <button
          onClick={() => navigate(`/search?universityId=${uniId}`)}
          className="text-sm text-primary hover:underline"
        >
          Tümünü gör →
        </button>
      </div>

      {professors?.items.length === 0 ? (
        <div className="card p-10 text-center text-text-muted">
          Bu üniversitede henüz hoca kaydı yok.
        </div>
      ) : (
        <div className="grid sm:grid-cols-2 gap-3">
          {professors?.items.map(p => (
            <ProfessorCard key={p.id} professor={p} />
          ))}
        </div>
      )}

      {/* Fakülteler */}
      {u.faculties && u.faculties.length > 0 && (
        <div className="mt-8">
          <h2 className="font-display text-xl text-text mb-4">Fakülteler</h2>
          <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-2">
            {u.faculties.map(f => (
              <div key={f.id} className="card px-4 py-3">
                <p className="text-sm font-medium text-text">{f.name}</p>
                {f.departments?.length > 0 && (
                  <p className="text-xs text-text-muted mt-0.5">
                    {f.departments.length} bölüm
                  </p>
                )}
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}
