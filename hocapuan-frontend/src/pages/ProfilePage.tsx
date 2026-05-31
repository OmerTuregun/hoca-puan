import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { authApi, reviewApi } from '../services/api'
import { useAuthStore } from '../store/authStore'
import RatingBadge from '../components/ui/RatingBadge'
import Spinner from '../components/ui/Spinner'
import { Pencil } from 'lucide-react'

export default function ProfilePage() {
  const navigate = useNavigate()
  const { isLoggedIn, hasHydrated } = useAuthStore()
  const [page, setPage] = useState(1)

  useEffect(() => {
    if (!hasHydrated) return
    if (!isLoggedIn) {
      navigate('/login', { replace: true, state: { from: '/profile' } })
    }
  }, [hasHydrated, isLoggedIn, navigate])

  const { data: profile, isLoading: profileLoading } = useQuery({
    queryKey: ['auth', 'me'],
    queryFn: () => authApi.me(),
    enabled: hasHydrated && isLoggedIn,
  })

  const { data: reviews, isLoading: reviewsLoading } = useQuery({
    queryKey: ['reviews', 'my', page],
    queryFn: () => reviewApi.myReviews(page, 10),
    enabled: hasHydrated && isLoggedIn,
  })

  if (!hasHydrated || !isLoggedIn) return <Spinner />
  if (profileLoading) return <Spinner />

  const memberSince = profile?.createdAt
    ? new Date(profile.createdAt).toLocaleDateString('tr-TR', {
        day: 'numeric',
        month: 'long',
        year: 'numeric',
      })
    : '—'

  return (
    <div className="max-w-3xl mx-auto px-4 py-8">
      <h1 className="font-display text-3xl text-text mb-6">Profilim</h1>

      {profile && (
        <div className="card p-6 mb-8">
          <p className="font-display text-2xl text-text">{profile.username}</p>
          <p className="text-text-muted text-sm mt-1">{profile.email}</p>
          {profile.universityName && (
            <p className="text-sm text-text-muted mt-2">{profile.universityName}</p>
          )}
          <div className="flex flex-wrap gap-6 mt-4 pt-4 border-t border-surface-border text-sm">
            <div>
              <span className="text-text-muted block">Üyelik</span>
              <span className="font-medium text-text">{memberSince}</span>
            </div>
            <div>
              <span className="text-text-muted block">Toplam yorum</span>
              <span className="font-medium text-text">{profile.totalReviews}</span>
            </div>
          </div>
        </div>
      )}

      <h2 className="font-display text-xl text-text mb-4">Yorumlarım</h2>

      {reviewsLoading ? (
        <Spinner />
      ) : reviews?.items.length === 0 ? (
        <div className="card p-10 text-center text-text-muted">
          <p>Henüz yorum yazmadınız.</p>
          <Link to="/search" className="btn-primary mt-4 inline-flex">Hoca ara</Link>
        </div>
      ) : (
        <div className="flex flex-col gap-3">
          {reviews?.items.map(r => (
            <article key={r.id} className="card p-5">
              <div className="flex flex-wrap items-start justify-between gap-3 mb-3">
                <div>
                  <Link
                    to={`/professors/${r.professorId}`}
                    className="font-semibold text-text hover:text-primary transition-colors"
                  >
                    {r.professorFullName}
                  </Link>
                  <p className="text-sm text-text-muted mt-0.5">{r.universityName}</p>
                </div>
                <Link
                  to={`/reviews/${r.id}/edit`}
                  state={{ from: '/profile' }}
                  className="btn-outline text-sm py-1.5 px-3 inline-flex items-center gap-1.5"
                >
                  <Pencil className="w-3.5 h-3.5" />
                  Düzenle
                </Link>
              </div>

              <div className="flex flex-wrap gap-4 mb-3">
                <RatingBadge value={r.qualityRating} size="sm" label="Kalite" />
                <RatingBadge value={r.difficultyRating} size="sm" label="Zorluk" />
              </div>

              <p className="text-sm text-text leading-relaxed">{r.comment}</p>

              <p className="text-xs text-text-muted mt-3">
                {new Date(r.createdAt).toLocaleDateString('tr-TR', {
                  day: 'numeric',
                  month: 'long',
                  year: 'numeric',
                })}
              </p>
            </article>
          ))}

          {reviews && reviews.totalPages > 1 && (
            <div className="flex justify-center gap-2 mt-4">
              <button
                type="button"
                onClick={() => setPage(p => Math.max(1, p - 1))}
                disabled={!reviews.hasPreviousPage}
                className="btn-outline disabled:opacity-40"
              >
                Önceki
              </button>
              <span className="btn-ghost cursor-default">{page} / {reviews.totalPages}</span>
              <button
                type="button"
                onClick={() => setPage(p => p + 1)}
                disabled={!reviews.hasNextPage}
                className="btn-outline disabled:opacity-40"
              >
                Sonraki
              </button>
            </div>
          )}
        </div>
      )}
    </div>
  )
}
