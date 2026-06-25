import { useEffect, useState } from 'react'
import { Link, useNavigate, useLocation } from 'react-router-dom'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { authApi, reviewApi, userApi } from '../services/api'
import { useAuthStore } from '../store/authStore'
import RatingBadge from '../components/ui/RatingBadge'
import Spinner from '../components/ui/Spinner'
import ConfirmModal from '../components/ui/ConfirmModal'
import { Pencil, Trash2 } from 'lucide-react'
import { parseReviewDeleteError } from '../utils/reviewDeleteError'

export default function ProfilePage() {
  const navigate = useNavigate()
  const location = useLocation()
  const qc = useQueryClient()
  const { isLoggedIn, hasHydrated } = useAuthStore()
  const [page, setPage] = useState(1)
  const [reviewUpdated, setReviewUpdated] = useState(
    () => !!(location.state as { reviewUpdated?: boolean } | null)?.reviewUpdated
  )
  const [reviewPending, setReviewPending] = useState(
    () => !!(location.state as { reviewPending?: boolean } | null)?.reviewPending
  )
  const [reviewInfoMessage, setReviewInfoMessage] = useState(
    () => (location.state as { reviewInfoMessage?: string } | null)?.reviewInfoMessage ?? ''
  )
  const [deletingId, setDeletingId] = useState<number | null>(null)
  const [deleteError, setDeleteError] = useState('')
  const [pendingDeleteId, setPendingDeleteId] = useState<number | null>(null)

  useEffect(() => {
    if (!hasHydrated) return
    if (!isLoggedIn) {
      navigate('/login', { replace: true, state: { from: '/profile' } })
    }
  }, [hasHydrated, isLoggedIn, navigate])

  useEffect(() => {
    const state = location.state as {
      reviewUpdated?: boolean
      reviewPending?: boolean
      reviewInfoMessage?: string
    } | null
    if (state?.reviewUpdated) {
      setReviewUpdated(true)
      setReviewPending(!!state.reviewPending)
      setReviewInfoMessage(state.reviewInfoMessage ?? '')
      void Promise.all([
        qc.invalidateQueries({ queryKey: ['users', 'me', 'contributions'] }),
        qc.invalidateQueries({ queryKey: ['auth', 'me'] }),
      ])
      window.history.replaceState({}, document.title, location.pathname)
    }
  }, [location.state, location.pathname, qc])

  const { data: profile, isLoading: profileLoading } = useQuery({
    queryKey: ['auth', 'me'],
    queryFn: () => authApi.me(),
    enabled: hasHydrated && isLoggedIn,
  })

  const { data: contributions, isLoading: reviewsLoading } = useQuery({
    queryKey: ['users', 'me', 'contributions', page],
    queryFn: () => userApi.contributions(page, 10),
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

  async function confirmDeleteReview() {
    if (pendingDeleteId == null) return

    setDeletingId(pendingDeleteId)
    setDeleteError('')
    try {
      await reviewApi.delete(pendingDeleteId)
      setPendingDeleteId(null)
      await Promise.all([
        qc.invalidateQueries({ queryKey: ['users', 'me', 'contributions'] }),
        qc.invalidateQueries({ queryKey: ['auth', 'me'] }),
      ])
    } catch (error: unknown) {
      setDeleteError(parseReviewDeleteError(error))
      setPendingDeleteId(null)
    } finally {
      setDeletingId(null)
    }
  }

  return (
    <div className="max-w-3xl mx-auto px-4 py-8">
      <ConfirmModal
        open={pendingDeleteId != null}
        title="Yorumu sil"
        message="Bu yorumu silmek istediğinize emin misiniz?"
        loading={deletingId != null}
        onCancel={() => deletingId == null && setPendingDeleteId(null)}
        onConfirm={() => void confirmDeleteReview()}
      />
      <h1 className="font-display text-3xl text-text mb-6">Profilim</h1>

      {reviewUpdated && (
        <div
          className={`mb-4 rounded-lg px-4 py-3 text-sm flex items-start justify-between gap-3 ${
            reviewPending
              ? 'bg-amber-50 border border-amber-200 text-amber-900'
              : 'bg-green-50 border border-green-200 text-green-800'
          }`}
        >
          <span>
            {reviewInfoMessage ||
              (reviewPending
                ? 'Yorumunuz incelemeye alındı, onaylandığında yayınlanacaktır.'
                : 'Yorumunuz başarıyla güncellendi.')}
          </span>
          <button
            type="button"
            onClick={() => setReviewUpdated(false)}
            className={`shrink-0 ${reviewPending ? 'text-amber-800 hover:text-amber-950' : 'text-green-700 hover:text-green-900'}`}
            aria-label="Kapat"
          >
            ×
          </button>
        </div>
      )}

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
              <span className="font-medium text-text">{contributions?.totalReviews ?? profile.totalReviews}</span>
            </div>
            <div>
              <span className="text-text-muted block">Faydalı bulundu</span>
              <span className="font-medium text-text">{contributions?.totalHelpfulVotes ?? '—'}</span>
            </div>
          </div>
        </div>
      )}

      <h2 className="font-display text-xl text-text mb-4">Yorumlarım</h2>

      {deleteError && (
        <div className="mb-4 bg-red-50 border border-red-200 rounded-lg p-3 text-sm text-red-700">
          {deleteError}
        </div>
      )}

      {reviewsLoading ? (
        <Spinner />
      ) : contributions?.reviews.items.length === 0 ? (
        <div className="card p-10 text-center text-text-muted">
          <p>Henüz yorum yazmadınız.</p>
          <Link to="/search" className="btn-primary mt-4 inline-flex">Hoca ara</Link>
        </div>
      ) : (
        <div className="flex flex-col gap-3">
          {contributions?.reviews.items.map(r => (
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
                <div className="flex items-center gap-2">
                  {r.status === 'Pending' && (
                    <span className="text-xs bg-amber-50 text-amber-800 border border-amber-200 rounded-full px-2 py-0.5">
                      İncelemede
                    </span>
                  )}
                  {r.status === 'Rejected' && (
                    <span className="text-xs bg-red-50 text-red-700 border border-red-200 rounded-full px-2 py-0.5">
                      Reddedildi
                    </span>
                  )}
                  <Link
                    to={`/reviews/${r.id}/edit`}
                    state={{ from: '/profile' }}
                    className="btn-outline text-sm py-1.5 px-3 inline-flex items-center gap-1.5"
                  >
                    <Pencil className="w-3.5 h-3.5" />
                    Düzenle
                  </Link>
                  <button
                    type="button"
                    onClick={() => setPendingDeleteId(r.id)}
                    disabled={deletingId === r.id}
                    className="btn-outline text-sm py-1.5 px-3 inline-flex items-center gap-1.5 text-red-600 border-red-200 hover:bg-red-50 disabled:opacity-50"
                    aria-label="Yorumu sil"
                  >
                    <Trash2 className="w-3.5 h-3.5" />
                    Sil
                  </button>
                </div>
              </div>

              <div className="flex flex-wrap gap-4 mb-3">
                <RatingBadge value={r.qualityRating} size="sm" label="Kalite" />
                <RatingBadge value={r.difficultyRating} size="sm" label="Zorluk" />
              </div>

              <p className="text-sm text-text leading-relaxed">{r.comment}</p>

              <div className="flex items-center justify-between mt-3">
                <p className="text-xs text-text-muted">
                  {new Date(r.createdAt).toLocaleDateString('tr-TR', {
                    day: 'numeric',
                    month: 'long',
                    year: 'numeric',
                  })}
                </p>
                <p className="text-xs text-text-muted">
                  👍 {r.thumbsUp} · 👎 {r.thumbsDown}
                </p>
              </div>
            </article>
          ))}

          {contributions && contributions.reviews.totalPages > 1 && (
            <div className="flex justify-center gap-2 mt-4">
              <button
                type="button"
                onClick={() => setPage(p => Math.max(1, p - 1))}
                disabled={!contributions.reviews.hasPreviousPage}
                className="btn-outline disabled:opacity-40"
              >
                Önceki
              </button>
              <span className="btn-ghost cursor-default">{page} / {contributions.reviews.totalPages}</span>
              <button
                type="button"
                onClick={() => setPage(p => p + 1)}
                disabled={!contributions.reviews.hasNextPage}
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
