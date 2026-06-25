import { useMemo } from 'react'
import { useParams, Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { Helmet } from 'react-helmet-async'
import { Building2, ChevronRight, BookOpen } from 'lucide-react'
import { universityApi, professorApi, type Professor } from '../services/api'
import Spinner from '../components/ui/Spinner'
import ProfessorCard from '../components/professor/ProfessorCard'

export default function FacultyPage() {
  const { universityId, facultyId } = useParams<{ universityId: string; facultyId: string }>()
  const uniId = Number(universityId)
  const facId = Number(facultyId)

  const { data: university, isLoading: uniLoading } = useQuery({
    queryKey: ['university', uniId],
    queryFn: () => universityApi.get(uniId),
    enabled: !!uniId,
  })

  const { data: faculties, isLoading: facLoading } = useQuery({
    queryKey: ['university-faculties', uniId],
    queryFn: () => universityApi.faculties(uniId),
    enabled: !!uniId,
  })

  const faculty = useMemo(
    () => faculties?.find(f => f.id === facId),
    [faculties, facId],
  )

  const { data: professors, isLoading: profLoading } = useQuery({
    queryKey: ['professors-faculty', uniId, facId],
    queryFn: () => professorApi.search({ universityId: uniId, facultyId: facId, pageSize: 100 }),
    enabled: !!uniId && !!facId,
  })

  if (uniLoading || facLoading) return <Spinner />

  if (!university || !faculty) {
    return (
      <div className="max-w-3xl mx-auto px-4 py-20 text-center text-text-muted">
        Fakülte bulunamadı.
      </div>
    )
  }

  const profList = professors?.items ?? []
  const pageTitle = `${faculty.name} - ${university.name} | Hocanı Yorumla`
  const pageDescription = `${university.name} ${faculty.name} fakültesinde ${faculty.totalProfessors} hoca. Bölümlere ve hocalara göz at.`

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
        <nav className="flex items-center gap-1.5 text-sm text-text-muted mb-6">
          <Link to="/universities" className="hover:text-primary transition-colors">Üniversiteler</Link>
          <ChevronRight className="w-3.5 h-3.5" />
          <Link to={`/universities/${uniId}`} className="hover:text-primary transition-colors truncate max-w-[200px]">
            {university.name}
          </Link>
          <ChevronRight className="w-3.5 h-3.5" />
          <span className="text-text truncate">{faculty.name}</span>
        </nav>

        <div className="card p-6 mb-6 animate-fadeUp">
          <div className="flex items-start gap-4">
            <div className="w-14 h-14 rounded-xl bg-primary-light flex items-center justify-center text-primary shrink-0">
              <Building2 className="w-7 h-7" />
            </div>
            <div className="flex-1">
              <p className="text-sm text-text-muted">{university.name}</p>
              <h1 className="font-display text-2xl text-text mt-0.5">{faculty.name}</h1>
            </div>
          </div>

          <div className="mt-5 pt-5 border-t border-surface-border grid grid-cols-2 gap-4 text-center">
            <div>
              <p className="text-xl font-bold text-text">{faculty.departments?.length ?? 0}</p>
              <p className="text-xs text-text-muted mt-0.5">Bölüm</p>
            </div>
            <div>
              <p className="text-xl font-bold text-text">{faculty.totalProfessors}</p>
              <p className="text-xs text-text-muted mt-0.5">Hoca</p>
            </div>
          </div>
        </div>

        {faculty.departments && faculty.departments.length > 0 && (
          <section className="mb-8">
            <h2 className="font-display text-xl text-text mb-3">Bölümler</h2>
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
              {faculty.departments.map(d => (
                <Link
                  key={d.id}
                  to={`/universities/${uniId}/departments/${d.id}`}
                  className="card-hover p-4 text-left"
                >
                  <div className="flex items-start gap-3">
                    <div className="w-10 h-10 rounded-lg bg-primary-light flex items-center justify-center text-primary shrink-0">
                      <BookOpen className="w-5 h-5" />
                    </div>
                    <p className="font-semibold text-text text-sm leading-tight">{d.name}</p>
                  </div>
                </Link>
              ))}
            </div>
          </section>
        )}

        <section>
          <h2 className="font-display text-xl text-text mb-3">Hocalar</h2>
          {profLoading ? (
            <Spinner />
          ) : profList.length === 0 ? (
            <div className="card p-10 text-center text-text-muted">
              Bu fakültede henüz hoca kaydı yok.
            </div>
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
              {profList.map((p: Professor) => (
                <ProfessorCard key={p.id} professor={p} />
              ))}
            </div>
          )}
        </section>
      </div>
    </>
  )
}
