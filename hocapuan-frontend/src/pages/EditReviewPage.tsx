import { useParams, useNavigate, useLocation, Link } from 'react-router-dom'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { reviewApi } from '../services/api'
import { useAuthStore } from '../store/authStore'
import Spinner from '../components/ui/Spinner'
import ReviewEditForm from '../components/review/ReviewEditForm'
import { useEffect } from 'react'

export default function EditReviewPage() {
  const { id } = useParams<{ id: string }>()
  const reviewId = Number(id)
  const navigate = useNavigate()
  const location = useLocation()
  const fromPath = (location.state as { from?: string } | null)?.from
  const qc = useQueryClient()
  const { user, isLoggedIn, hasHydrated } = useAuthStore()

  useEffect(() => {
    if (!hasHydrated) return
    if (!isLoggedIn) {
      navigate('/login', { replace: true, state: { from: `/reviews/${reviewId}/edit` } })
    }
  }, [hasHydrated, isLoggedIn, navigate, reviewId])

  const { data: review, isLoading, isError } = useQuery({
    queryKey: ['review', reviewId],
    queryFn: () => reviewApi.get(reviewId),
    enabled: !!reviewId && hasHydrated && isLoggedIn,
  })

  useEffect(() => {
    if (!review || !user) return
    if (review.userId !== user.id) {
      navigate('/profile', { replace: true })
    }
  }, [review, user, navigate])

  if (!hasHydrated || !isLoggedIn) return <Spinner />
  if (isLoading) return <Spinner />
  if (isError || !review) {
    return (
      <div className="max-w-2xl mx-auto px-4 py-20 text-center">
        <p className="text-text-muted">Yorum bulunamadı.</p>
        <Link to={fromPath ?? '/profile'} className="btn-primary mt-4 inline-flex">Geri dön</Link>
      </div>
    )
  }

  const backTo = fromPath ?? `/professors/${review.professorId}`
  const backLabel = backTo === '/profile' ? 'Profilim' : 'Hoca sayfası'

  return (
    <div className="max-w-2xl mx-auto px-4 py-8">
      <Link to={backTo} className="text-sm text-text-muted hover:text-primary mb-4 inline-block">
        ← {backLabel}
      </Link>
      <h1 className="font-display text-3xl text-text mb-2">Yorumu düzenle</h1>
      <p className="text-text-muted text-sm mb-6">{review.professorFullName}</p>

      <ReviewEditForm
        review={review}
        onCancel={() => navigate(backTo)}
        onSuccess={async (updated) => {
          await Promise.all([
            qc.invalidateQueries({ queryKey: ['reviews', review.professorId] }),
            qc.invalidateQueries({ queryKey: ['professor', review.professorId] }),
            qc.invalidateQueries({ queryKey: ['reviews', 'my'] }),
            qc.invalidateQueries({ queryKey: ['auth', 'me'] }),
            qc.invalidateQueries({ queryKey: ['review', reviewId] }),
          ])
          const destination = fromPath ?? `/professors/${review.professorId}`
          navigate(destination, {
            replace: true,
            state: destination === '/profile'
              ? { reviewUpdated: true, reviewPending: updated.status === 'Pending', reviewInfoMessage: updated.infoMessage }
              : {
                  reviewSuccess: true,
                  reviewPending: updated.status === 'Pending',
                  reviewInfoMessage: updated.infoMessage,
                },
          })
        }}
      />
    </div>
  )
}
