import { useMemo } from 'react'
import { useParams, Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { Helmet } from 'react-helmet-async'
import { BookOpen, ChevronRight } from 'lucide-react'
import { universityApi, type DepartmentProfessor } from '../services/api'
import { slugify } from '../utils/slugify'
import Spinner from '../components/ui/Spinner'
import RatingBadge from '../components/ui/RatingBadge'
import { displayProfessorName, professorInitials } from '../utils/professorDisplay'

function ProfessorRow({ p }: { p: DepartmentProfessor }) {
  const displayName = displayProfessorName(p)
  const initials = professorInitials(p)

  return (
    <Link
      to={`/professors/${p.id}`}
      className="card-hover flex items-center gap-4 p-4"
    >
      <div className="w-11 h-11 rounded-xl bg-primary-light flex items-center justify-center shrink-0 text-primary font-bold">
        {initials}
      </div>
      <div className="flex-1 min-w-0">
        <p className="text-xs text-text-muted font-medium">{p.title}</p>
        <p className="font-semibold text-text truncate">{displayName}</p>
        {p.totalReviews > 0 && (
          <div className="mt-1 flex flex-wrap gap-x-3 gap-y-0.5 text-xs text-text-muted">
            <span><span className="font-semibold text-text">{p.totalReviews}</span> yorum</span>
            <span>Zorluk: <span className="font-semibold text-text">{p.averageDifficulty.toFixed(1)}</span>/5</span>
            <span>Tekrar alır: <span className="font-semibold text-green-600">{p.wouldTakeAgainPercent}%</span></span>
          </div>
        )}
      </div>
      <RatingBadge value={p.averageQuality} size="sm" />
    </Link>
  )
}

export default function DepartmentPage() {
  const { universityId: universityParam, departmentId: departmentParam } = useParams<{
    universityId: string
    departmentId: string
  }>()

  const isUniNumeric = /^\d+$/.test(universityParam ?? '')
  const isDeptNumeric = /^\d+$/.test(departmentParam ?? '')

  // When universityParam is a slug, resolve it by matching slugified university names
  const { data: allUniversities } = useQuery({
    queryKey: ['universities-list'],
    queryFn: () => universityApi.list(),
    enabled: !isUniNumeric,
    staleTime: 5 * 60 * 1000,
  })

  const uniId = useMemo((): number | null | undefined => {
    if (isUniNumeric) return Number(universityParam)
    if (!allUniversities) return undefined // still loading
    const match = allUniversities.find(u => slugify(u.name) === universityParam)
    return match ? match.id : null // null = not found
  }, [isUniNumeric, universityParam, allUniversities])

  // When departmentParam is a slug, resolve it by matching slugified department names
  const { data: faculties } = useQuery({
    queryKey: ['university-faculties-for-slug', uniId],
    queryFn: () => universityApi.faculties(uniId!),
    enabled: typeof uniId === 'number' && !isDeptNumeric,
    staleTime: 5 * 60 * 1000,
  })

  const deptId = useMemo((): number | null | undefined => {
    if (isDeptNumeric) return Number(departmentParam)
    if (typeof uniId !== 'number') return undefined // wait for university resolution
    if (!faculties) return undefined // still loading
    for (const faculty of faculties) {
      const match = faculty.departments?.find(d => slugify(d.name) === departmentParam)
      if (match) return match.id
    }
    return null // not found
  }, [isDeptNumeric, departmentParam, faculties, uniId])

  const canFetch = typeof uniId === 'number' && typeof deptId === 'number'
  const isResolving = uniId === undefined || deptId === undefined

  const { data: dept, isLoading, isError } = useQuery({
    queryKey: ['department-detail', uniId, deptId],
    queryFn: () => universityApi.departmentDetail(uniId!, deptId!),
    enabled: canFetch,
  })

  if (isResolving || (canFetch && isLoading)) return <Spinner />

  if (uniId === null || deptId === null || isError || !dept) {
    return (
      <div className="max-w-3xl mx-auto px-4 py-20 text-center text-text-muted">
        Bölüm bulunamadı.
      </div>
    )
  }

  const pageTitle = `${dept.name} Hocaları - ${dept.universityName} | Hocanı Yorumla`
  const pageDescription = `${dept.universityName} ${dept.facultyName} ${dept.name} bölümünde ${dept.totalProfessors} hoca, ${dept.totalReviews} öğrenci yorumu. Hoca puanlarını keşfet.`
  const canonicalUrl = `/universite/${slugify(dept.universityName)}/bolum/${slugify(dept.name)}`

  return (
    <>
      <Helmet>
        <title>{pageTitle}</title>
        <meta name="description" content={pageDescription} />
        <meta property="og:title" content={pageTitle} />
        <meta property="og:description" content={pageDescription} />
        <meta property="og:type" content="website" />
        <link rel="canonical" href={canonicalUrl} />
      </Helmet>

      <div className="max-w-3xl mx-auto px-4 py-8">
        {/* Breadcrumb */}
        <nav className="flex items-center gap-1.5 text-sm text-text-muted mb-6">
          <Link to="/universities" className="hover:text-primary transition-colors">Üniversiteler</Link>
          <ChevronRight className="w-3.5 h-3.5" />
          <Link to={`/universities/${dept.universityId}`} className="hover:text-primary transition-colors truncate max-w-[200px]">
            {dept.universityName}
          </Link>
          <ChevronRight className="w-3.5 h-3.5" />
          <span className="text-text truncate">{dept.name}</span>
        </nav>

        {/* Header */}
        <div className="card p-6 mb-6 animate-fadeUp">
          <div className="flex items-start gap-4">
            <div className="w-14 h-14 rounded-xl bg-primary-light flex items-center justify-center text-primary shrink-0">
              <BookOpen className="w-7 h-7" />
            </div>
            <div className="flex-1">
              <p className="text-sm text-text-muted">{dept.facultyName}</p>
              <h1 className="font-display text-2xl text-text mt-0.5">{dept.name}</h1>
              <p className="text-sm text-text-muted mt-1">{dept.universityName}</p>
            </div>
          </div>

          <div className="mt-5 pt-5 border-t border-surface-border grid grid-cols-4 gap-3 text-center">
            <div>
              <p className="text-xl font-bold text-text">{dept.totalProfessors}</p>
              <p className="text-xs text-text-muted mt-0.5">Hoca</p>
            </div>
            <div>
              <p className="text-xl font-bold text-text">{dept.totalReviews}</p>
              <p className="text-xs text-text-muted mt-0.5">Yorum</p>
            </div>
            <div>
              <p className="text-xl font-bold text-primary">
                {dept.avgQuality > 0 ? dept.avgQuality.toFixed(1) : '—'}
              </p>
              <p className="text-xs text-text-muted mt-0.5">Ort. kalite</p>
            </div>
            <div>
              <p className="text-xl font-bold text-text">
                {dept.avgDifficulty > 0 ? dept.avgDifficulty.toFixed(1) : '—'}
              </p>
              <p className="text-xs text-text-muted mt-0.5">Ort. zorluk</p>
            </div>
          </div>
        </div>

        {/* Professor list */}
        <h2 className="font-display text-xl text-text mb-3">Hocalar</h2>

        {dept.professors.length === 0 ? (
          <div className="card p-10 text-center text-text-muted">
            Bu bölümde henüz hoca kaydı yok.
          </div>
        ) : (
          <div className="flex flex-col gap-2">
            {dept.professors.map(p => (
              <ProfessorRow key={p.id} p={p} />
            ))}
          </div>
        )}
      </div>
    </>
  )
}
