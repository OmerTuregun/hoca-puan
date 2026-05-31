import { useParams, Link, useLocation } from 'react-router-dom'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { MapPin, BookOpen, Globe, Mail, MessageSquarePlus, GraduationCap, ChevronRight } from 'lucide-react'
import { professorApi, reviewApi } from '../services/api'
import { useAuthStore } from '../store/authStore'
import RatingBadge from '../components/ui/RatingBadge'
import ReviewCard from '../components/review/ReviewCard'
import Spinner from '../components/ui/Spinner'
import { useState, useEffect } from 'react'

export default function ProfessorPage() {
  const { id } = useParams<{ id: string }>()
  const profId = Number(id)
  const { isLoggedIn, hasHydrated } = useAuthStore()
  const authed = hasHydrated && isLoggedIn
  const qc = useQueryClient()
  const location = useLocation()
  const [page, setPage] = useState(1)
  const [reviewSuccess, setReviewSuccess] = useState(
    () => !!(location.state as { reviewSuccess?: boolean } | null)?.reviewSuccess
  )

  const reviewPath = `/professors/${profId}/review`

  const { data: professor, isLoading: profLoading } = useQuery({
    queryKey: ['professor', profId],
    queryFn:  () => professorApi.get(profId),
    enabled:  !!profId,
  })

  const { data: reviews, isLoading: revLoading } = useQuery({
    queryKey: ['reviews', profId, page],
    queryFn:  () => reviewApi.byProfessor(profId, page, 10),
    enabled:  !!profId,
  })

  useEffect(() => {
    if (!professor) return
    document.title = `${professor.firstName} ${professor.lastName} — HocaPuan`
    return () => { document.title = 'HocaPuan' }
  }, [professor])

  useEffect(() => {
    if ((location.state as { reviewSuccess?: boolean } | null)?.reviewSuccess) {
      setReviewSuccess(true)
      setPage(1)
      void Promise.all([
        qc.invalidateQueries({ queryKey: ['reviews', profId] }),
        qc.invalidateQueries({ queryKey: ['professor', profId] }),
      ])
      window.history.replaceState({}, document.title, location.pathname)
    }
  }, [location.state, location.pathname, profId, qc])

  if (profLoading) return <Spinner />
  if (!professor)  return (
    <div className="max-w-3xl mx-auto px-4 py-16 text-center">
      <p className="text-text-muted">Hoca bulunamadı.</p>
      <Link to="/search" className="btn-primary mt-4 inline-flex">Geri dön</Link>
    </div>
  )

  const p = professor

  return (
    <div className="max-w-4xl mx-auto px-4 py-8">

      {/* Breadcrumb */}
      <nav className="flex flex-wrap items-center gap-1 text-sm text-text-muted mb-4">
        <Link to="/" className="hover:text-primary transition-colors">Ana Sayfa</Link>
        <ChevronRight className="w-3.5 h-3.5 shrink-0" />
        <Link to={`/universities/${p.universityId}`} className="hover:text-primary transition-colors">
          {p.universityName}
        </Link>
        <ChevronRight className="w-3.5 h-3.5 shrink-0" />
        <span className="text-text font-medium">{p.firstName} {p.lastName}</span>
      </nav>

      {reviewSuccess && (
        <div className="mb-4 bg-green-50 border border-green-200 rounded-lg px-4 py-3 text-sm text-green-800 flex items-start justify-between gap-3">
          <span>Yorumun başarıyla gönderildi.</span>
          <button
            type="button"
            onClick={() => setReviewSuccess(false)}
            className="text-green-700 hover:text-green-900 shrink-0"
            aria-label="Kapat"
          >
            ×
          </button>
        </div>
      )}

      {/* ─── Profil kartı ─── */}
      <div className="card p-6 mb-6 animate-fadeUp">
        <div className="flex flex-col sm:flex-row gap-6">
          {/* Avatar */}
          <div className="w-20 h-20 rounded-2xl bg-primary-light flex items-center justify-center text-primary font-bold text-2xl shrink-0">
            {p.firstName?.[0]}{p.lastName?.[0]}
          </div>

          {/* Bilgi */}
          <div className="flex-1">
            <p className="text-sm text-text-muted font-medium">{p.title}</p>
            <h1 className="font-display text-3xl text-text">{p.firstName} {p.lastName}</h1>

            <div className="mt-2 flex flex-wrap gap-x-4 gap-y-1 text-sm text-text-muted">
              <Link
                to={`/universities/${p.universityId}`}
                className="flex items-center gap-1.5 hover:text-primary transition-colors"
              >
                <MapPin className="w-4 h-4" />{p.universityName}
              </Link>
              {p.facultyName && (
                <span className="flex items-center gap-1.5">
                  <GraduationCap className="w-4 h-4" />{p.facultyName}
                </span>
              )}
              <span className="flex items-center gap-1.5">
                <BookOpen className="w-4 h-4" />{p.departmentName}
              </span>
              {p.email && (
                <a href={`mailto:${p.email}`} className="flex items-center gap-1.5 hover:text-primary transition-colors">
                  <Mail className="w-4 h-4" />{p.email}
                </a>
              )}
              {p.personalWebsite && (
                <a href={p.personalWebsite} target="_blank" rel="noopener" className="flex items-center gap-1.5 hover:text-primary transition-colors">
                  <Globe className="w-4 h-4" />Web sitesi
                </a>
              )}
            </div>
          </div>

          {/* Yorum butonu */}
          <div className="shrink-0">
            {!hasHydrated ? (
              <span className="btn-outline opacity-60 cursor-wait">Yükleniyor...</span>
            ) : authed ? (
              <Link to={reviewPath} className="btn-primary">
                <MessageSquarePlus className="w-4 h-4" />
                Yorum yaz
              </Link>
            ) : (
              <Link to="/login" state={{ from: reviewPath }} className="btn-outline">
                Giriş yapıp yorum yaz
              </Link>
            )}
          </div>
        </div>

        {/* ─── İstatistikler ─── */}
        {p.totalReviews > 0 ? (
          <div className="mt-6 pt-6 border-t border-surface-border grid grid-cols-2 sm:grid-cols-4 gap-4">
            <RatingBadge value={p.averageQuality}    size="lg" label="Kalite" />
            <RatingBadge value={p.averageDifficulty} size="lg" label="Zorluk" />
            <div className="flex flex-col items-center gap-1">
              <div className="w-14 h-14 rounded-xl bg-green-50 flex items-center justify-center font-bold text-green-600 text-base">
                {p.wouldTakeAgainPercent}%
              </div>
              <span className="text-xs text-text-muted">Tekrar alır</span>
            </div>
            <div className="flex flex-col items-center gap-1">
              <div className="w-14 h-14 rounded-xl bg-surface-alt flex items-center justify-center font-bold text-text text-base">
                {p.totalReviews}
              </div>
              <span className="text-xs text-text-muted">Yorum</span>
            </div>
          </div>
        ) : (
          <div className="mt-6 pt-6 border-t border-surface-border text-center">
            <p className="text-text-muted font-medium">Henüz değerlendirilmedi</p>
            <p className="text-sm text-text-light mt-1">İlk yorumu sen yaz!</p>
          </div>
        )}
      </div>

      {/* ─── Yorumlar ─── */}
      <h2 className="font-display text-xl text-text mb-4">Yorumlar</h2>

      {revLoading ? (
        <Spinner />
      ) : reviews?.items.length === 0 ? (
        <div className="card p-10 text-center text-text-muted">
          <p>Henüz yorum yok.</p>
          {hasHydrated && authed ? (
            <Link to={reviewPath} className="btn-primary mt-4 inline-flex">
              İlk yorumu yaz
            </Link>
          ) : hasHydrated ? (
            <Link to="/login" state={{ from: reviewPath }} className="btn-outline mt-4 inline-flex">
              Giriş yapıp yorum yaz
            </Link>
          ) : null}
        </div>
      ) : (
        <div className="flex flex-col gap-3">
          {reviews?.items.map(r => (
            <ReviewCard
              key={r.id}
              review={r}
              onDelete={() => qc.invalidateQueries({ queryKey: ['reviews', profId] })}
              onVote={() => qc.invalidateQueries({ queryKey: ['reviews', profId] })}
            />
          ))}

          {/* Sayfalama */}
          {reviews && reviews.totalPages > 1 && (
            <div className="flex justify-center gap-2 mt-4">
              <button
                onClick={() => setPage(p => Math.max(1, p - 1))}
                disabled={!reviews.hasPreviousPage}
                className="btn-outline disabled:opacity-40"
              >Önceki</button>
              <span className="btn-ghost cursor-default">{page} / {reviews.totalPages}</span>
              <button
                onClick={() => setPage(p => p + 1)}
                disabled={!reviews.hasNextPage}
                className="btn-outline disabled:opacity-40"
              >Sonraki</button>
            </div>
          )}
        </div>
      )}
    </div>
  )
}
