import { useState } from 'react'
import { Link, Navigate } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ShieldAlert } from 'lucide-react'
import { reviewApi } from '../services/api'
import { useAuthStore } from '../store/authStore'
import Spinner from '../components/ui/Spinner'

export default function AdminModerationPage() {
  const { isAdmin, hasHydrated, isLoggedIn } = useAuthStore()
  const qc = useQueryClient()
  const [page, setPage] = useState(1)
  const [actingId, setActingId] = useState<number | null>(null)

  const { data, isLoading, isError } = useQuery({
    queryKey: ['reviews', 'pending', page],
    queryFn: () => reviewApi.pending(page, 20),
    enabled: hasHydrated && isLoggedIn && isAdmin,
  })

  const moderateMutation = useMutation({
    mutationFn: ({ id, approve }: { id: number; approve: boolean }) =>
      reviewApi.moderate(id, { approve }),
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: ['reviews', 'pending'] })
    },
    onSettled: () => setActingId(null),
  })

  if (!hasHydrated) return <Spinner />

  if (!isLoggedIn || !isAdmin) {
    return <Navigate to="/" replace />
  }

  if (isLoading) return <Spinner />

  if (isError) {
    return (
      <div className="max-w-4xl mx-auto px-4 py-16 text-center">
        <p className="text-text-muted">Bekleyen yorumlar yüklenemedi.</p>
        <Link to="/" className="btn-primary mt-4 inline-flex">Ana sayfaya dön</Link>
      </div>
    )
  }

  const reviews = data?.items ?? []

  return (
    <div className="max-w-4xl mx-auto px-4 py-8">
      <div className="flex items-center gap-3 mb-6">
        <div className="w-10 h-10 rounded-xl bg-amber-100 flex items-center justify-center">
          <ShieldAlert className="w-5 h-5 text-amber-700" />
        </div>
        <div>
          <h1 className="font-display text-3xl text-text">Yorum moderasyonu</h1>
          <p className="text-sm text-text-muted mt-1">
            İnceleme bekleyen {data?.totalCount ?? 0} yorum
          </p>
        </div>
      </div>

      {reviews.length === 0 ? (
        <div className="card p-8 text-center text-text-muted">
          Bekleyen yorum yok.
        </div>
      ) : (
        <div className="flex flex-col gap-4">
          {reviews.map(review => (
            <article key={review.id} className="card p-5 flex flex-col gap-4">
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div>
                  <p className="font-medium text-text">{review.professorFullName}</p>
                  <p className="text-sm text-text-muted">{review.universityName}</p>
                  <p className="text-xs text-text-muted mt-1">
                    {review.username} · {new Date(review.createdAt).toLocaleString('tr-TR')}
                  </p>
                </div>
                <div className="flex gap-2">
                  <button
                    type="button"
                    disabled={actingId === review.id}
                    onClick={() => {
                      setActingId(review.id)
                      moderateMutation.mutate({ id: review.id, approve: true })
                    }}
                    className="btn-primary text-sm py-2 px-4"
                  >
                    Onayla
                  </button>
                  <button
                    type="button"
                    disabled={actingId === review.id}
                    onClick={() => {
                      setActingId(review.id)
                      moderateMutation.mutate({ id: review.id, approve: false })
                    }}
                    className="btn-ghost text-sm py-2 px-4 text-danger border border-red-200 hover:bg-red-50"
                  >
                    Reddet
                  </button>
                </div>
              </div>

              {review.manualReviewReasons && review.manualReviewReasons.length > 0 && (
                <div className="flex flex-wrap gap-2">
                  {review.manualReviewReasons.map(reason => (
                    <span
                      key={reason}
                      className="text-xs bg-amber-50 text-amber-800 border border-amber-200 rounded-full px-2.5 py-1"
                    >
                      {reason}
                    </span>
                  ))}
                </div>
              )}

              <p className="text-sm text-text whitespace-pre-wrap">{review.comment}</p>

              <div className="flex flex-wrap gap-3 text-xs text-text-muted">
                <span>Kalite: {review.qualityRating}/5</span>
                <span>Zorluk: {review.difficultyRating}/5</span>
                <span>Yıl: {review.year}</span>
                {review.courseCode && <span>Ders: {review.courseCode}</span>}
              </div>
            </article>
          ))}
        </div>
      )}

      {data && data.totalPages > 1 && (
        <div className="flex justify-center gap-2 mt-6">
          <button
            type="button"
            disabled={!data.hasPreviousPage}
            onClick={() => setPage(p => p - 1)}
            className="btn-ghost text-sm"
          >
            Önceki
          </button>
          <span className="text-sm text-text-muted self-center">
            {data.page} / {data.totalPages}
          </span>
          <button
            type="button"
            disabled={!data.hasNextPage}
            onClick={() => setPage(p => p + 1)}
            className="btn-ghost text-sm"
          >
            Sonraki
          </button>
        </div>
      )}
    </div>
  )
}
