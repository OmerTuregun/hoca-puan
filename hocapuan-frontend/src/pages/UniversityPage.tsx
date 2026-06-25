import { useParams, useNavigate, Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { Helmet } from 'react-helmet-async'
import { Globe, MapPin, TrendingUp, ChevronRight } from 'lucide-react'
import { universityApi, professorApi, type Professor } from '../services/api'
import { slugify } from '../utils/slugify'
import { displayProfessorName } from '../utils/professorDisplay'
import ProfessorCard from '../components/professor/ProfessorCard'
import Spinner from '../components/ui/Spinner'
import RatingBadge from '../components/ui/RatingBadge'

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
    queryFn:  () => professorApi.search({ universityId: uniId, pageSize: 50 }),
    enabled: !!uniId,
  })

  const { data: topProfessors } = useQuery({
    queryKey: ['university-top-professors', uniId],
    queryFn: () => universityApi.topProfessors(uniId, 10),
    enabled: !!uniId,
  })

  if (isLoading) return <Spinner />
  if (!university) return <div className="text-center py-20">Üniversite bulunamadı.</div>

  const u = university

  const pageTitle = `${u.name} Hocaları | Hocanı Yorumla`
  const pageDescription = `${u.name} bünyesinde ${u.totalProfessors} hoca ve ${u.totalReviews} öğrenci yorumu. Hoca puanlarını karşılaştır, bölümlere göre filtrele.`

  // Group professors by department
  const byDepartment = new Map<number, { deptId: number; deptName: string; professors: Professor[] }>()
  professors?.items.forEach(p => {
    if (!byDepartment.has(p.departmentId)) {
      byDepartment.set(p.departmentId, { deptId: p.departmentId, deptName: p.departmentName || 'Diğer', professors: [] })
    }
    byDepartment.get(p.departmentId)!.professors.push(p)
  })
  const departmentGroups = Array.from(byDepartment.values()).sort((a, b) =>
    a.deptName.localeCompare(b.deptName, 'tr')
  )

  return (
    <>
      <Helmet>
        <title>{pageTitle}</title>
        <meta name="description" content={pageDescription} />
        <meta property="og:title" content={pageTitle} />
        <meta property="og:description" content={pageDescription} />
        <meta property="og:type" content="website" />
      </Helmet>

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

        {topProfessors && topProfessors.length > 0 && (
          <div className="card p-5 mb-6">
            <div className="flex items-center gap-2 mb-4">
              <TrendingUp className="w-5 h-5 text-primary" />
              <h2 className="font-display text-lg text-text">En Çok Yorumlanan Hocalar</h2>
            </div>
            <div className="flex flex-col gap-2">
              {topProfessors.map((prof, index) => (
                <Link
                  key={prof.id}
                  to={`/professors/${prof.id}`}
                  className="flex items-center gap-3 p-3 rounded-xl border border-surface-border hover:border-primary hover:bg-primary-light/30 transition-colors min-h-[44px]"
                >
                  <span className="w-7 h-7 rounded-full bg-surface-alt text-text-muted text-sm font-semibold flex items-center justify-center shrink-0">
                    {index + 1}
                  </span>
                  <div className="flex-1 min-w-0">
                    <p className="font-medium text-text truncate">{displayProfessorName(prof)}</p>
                    <p className="text-xs text-text-muted truncate">
                      {[prof.facultyName, prof.departmentName].filter(Boolean).join(' · ')}
                    </p>
                  </div>
                  <div className="flex items-center gap-3 shrink-0">
                    <RatingBadge value={prof.averageQuality} size="sm" />
                    <span className="text-xs text-text-muted whitespace-nowrap">
                      {prof.totalReviews} yorum
                    </span>
                  </div>
                </Link>
              ))}
            </div>
          </div>
        )}

        {/* Bölümlere göre hocalar */}
        {departmentGroups.length > 0 ? (
          <div>
            {departmentGroups.map(group => (
              <div key={group.deptId} className="mb-8">
                <div className="flex items-center justify-between mb-3">
                  <Link
                    to={`/universite/${slugify(u.name)}/bolum/${slugify(group.deptName)}`}
                    className="flex items-center gap-1.5 group"
                  >
                    <h2 className="font-display text-lg text-text group-hover:text-primary transition-colors">
                      {group.deptName}
                    </h2>
                    <ChevronRight className="w-4 h-4 text-text-muted group-hover:text-primary transition-colors" />
                  </Link>
                  <span className="text-xs text-text-muted">{group.professors.length} hoca</span>
                </div>
                <div className="grid sm:grid-cols-2 gap-3">
                  {group.professors.map(p => (
                    <ProfessorCard key={p.id} professor={p} />
                  ))}
                </div>
              </div>
            ))}
          </div>
        ) : (
          <>
            <div className="flex items-center justify-between mb-4">
              <h2 className="font-display text-xl text-text">Hocalar</h2>
              <button
                onClick={() => navigate(`/search?universityId=${uniId}`)}
                className="text-sm text-primary hover:underline"
              >
                Tümünü gör →
              </button>
            </div>
            <div className="card p-10 text-center text-text-muted">
              Bu üniversitede henüz hoca kaydı yok.
            </div>
          </>
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
    </>
  )
}
