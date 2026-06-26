import { useState } from 'react'
import { useParams, Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { Helmet } from 'react-helmet-async'
import { Globe, MapPin, TrendingUp, Building2, BookOpen } from 'lucide-react'
import { universityApi } from '../services/api'
import { displayProfessorName } from '../utils/professorDisplay'
import Spinner from '../components/ui/Spinner'
import RatingBadge from '../components/ui/RatingBadge'

const FACULTY_INITIAL = 3
const FACULTY_STEP = 3
const DEPARTMENT_INITIAL = 6
const DEPARTMENT_STEP = 6

export default function UniversityPage() {
  const { id } = useParams<{ id: string }>()
  const uniId = Number(id)
  const [visibleFaculties, setVisibleFaculties] = useState(FACULTY_INITIAL)
  const [visibleDepartments, setVisibleDepartments] = useState(DEPARTMENT_INITIAL)

  const { data: university, isLoading } = useQuery({
    queryKey: ['university', uniId],
    queryFn:  () => universityApi.get(uniId),
  })

  const { data: faculties, isLoading: facultiesLoading } = useQuery({
    queryKey: ['university-faculties', uniId],
    queryFn: () => universityApi.faculties(uniId),
    enabled: !!uniId,
  })

  const { data: departments, isLoading: departmentsLoading } = useQuery({
    queryKey: ['university-departments', uniId],
    queryFn: () => universityApi.universityDepartments(uniId),
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
  const facultyList = faculties ?? []
  const departmentList = departments ?? []
  const shownFaculties = facultyList.slice(0, visibleFaculties)
  const shownDepartments = departmentList.slice(0, visibleDepartments)

  const pageTitle = `${u.name} Hocaları | Hocanı Yorumla`
  const pageDescription = `${u.name} bünyesinde ${u.totalProfessors} hoca ve ${u.totalReviews} öğrenci yorumu. Fakülte ve bölümlere göre keşfet.`

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

        {/* Fakülteler */}
        <section className="mb-8">
          <h2 className="font-display text-xl text-text mb-4">Fakülteler</h2>
          {facultiesLoading ? (
            <Spinner />
          ) : facultyList.length === 0 ? (
            <div className="card p-8 text-center text-text-muted text-sm">
              Bu üniversitede henüz fakülte kaydı yok.
            </div>
          ) : (
            <>
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
                {shownFaculties.map(f => (
                  <Link
                    key={f.id}
                    to={`/universities/${uniId}/faculties/${f.id}`}
                    className="card-hover p-4 text-left"
                  >
                    <div className="flex items-start gap-3">
                      <div className="w-10 h-10 rounded-lg bg-primary-light flex items-center justify-center text-primary shrink-0">
                        <Building2 className="w-5 h-5" />
                      </div>
                      <div className="min-w-0 flex-1">
                        <p className="font-semibold text-text text-sm leading-tight">{f.name}</p>
                        <p className="text-xs text-text-muted mt-1">
                          {f.departments?.length ?? 0} bölüm · {f.totalProfessors} hoca
                        </p>
                      </div>
                    </div>
                  </Link>
                ))}
              </div>
              {visibleFaculties < facultyList.length && (
                <div className="mt-4 text-center">
                  <button
                    type="button"
                    onClick={() => setVisibleFaculties(v => v + FACULTY_STEP)}
                    className="text-sm text-primary hover:underline font-medium"
                  >
                    Devamını Gör
                  </button>
                </div>
              )}
            </>
          )}
        </section>

        {/* Bölümler */}
        <section>
          <h2 className="font-display text-xl text-text mb-4">Bölümler</h2>
          {departmentsLoading ? (
            <Spinner />
          ) : departmentList.length === 0 ? (
            <div className="card p-8 text-center text-text-muted text-sm">
              Bu üniversitede henüz bölüm kaydı yok.
            </div>
          ) : (
            <>
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
                {shownDepartments.map(d => (
                  <Link
                    key={d.id}
                    to={`/universities/${uniId}/departments/${d.id}`}
                    className="card-hover p-4 text-left"
                  >
                    <div className="flex items-start gap-3">
                      <div className="w-10 h-10 rounded-lg bg-primary-light flex items-center justify-center text-primary shrink-0">
                        <BookOpen className="w-5 h-5" />
                      </div>
                      <div className="min-w-0 flex-1">
                        <p className="font-semibold text-text text-sm leading-tight">{d.name}</p>
                        <p className="text-xs text-text-muted mt-1 line-clamp-2" title={d.facultyName}>{d.facultyName}</p>
                        <p className="text-xs text-text-muted mt-0.5">{d.totalProfessors} hoca</p>
                      </div>
                    </div>
                  </Link>
                ))}
              </div>
              {visibleDepartments < departmentList.length && (
                <div className="mt-4 text-center">
                  <button
                    type="button"
                    onClick={() => setVisibleDepartments(v => v + DEPARTMENT_STEP)}
                    className="text-sm text-primary hover:underline font-medium"
                  >
                    Devamını Gör
                  </button>
                </div>
              )}
            </>
          )}
        </section>
      </div>
    </>
  )
}
