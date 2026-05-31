import { useParams, useNavigate, useLocation, Link } from 'react-router-dom'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useForm, Controller } from 'react-hook-form'
import { reviewApi } from '../services/api'
import { useAuthStore } from '../store/authStore'
import StarRating from '../components/ui/StarRating'
import Spinner from '../components/ui/Spinner'
import { REVIEW_TAGS } from '../constants/reviewTags'
import { useState, useEffect } from 'react'
import clsx from 'clsx'

interface FormData {
  qualityRating: number
  difficultyRating: number
  wouldTakeAgain: boolean
  attendanceMandatory: boolean
  courseCode: string
  grade: string
  year: number
  comment: string
}

export default function EditReviewPage() {
  const { id } = useParams<{ id: string }>()
  const reviewId = Number(id)
  const navigate = useNavigate()
  const location = useLocation()
  const fromPath = (location.state as { from?: string } | null)?.from
  const qc = useQueryClient()
  const { user, isLoggedIn, hasHydrated } = useAuthStore()
  const [selectedTags, setSelectedTags] = useState<string[]>([])
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')

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

  const { register, control, handleSubmit, reset, formState: { errors } } = useForm<FormData>()

  useEffect(() => {
    if (!review || !user) return
    if (review.userId !== user.id) {
      navigate('/profile', { replace: true })
      return
    }
    reset({
      qualityRating: review.qualityRating,
      difficultyRating: review.difficultyRating,
      wouldTakeAgain: review.wouldTakeAgain,
      attendanceMandatory: review.attendanceMandatory,
      courseCode: review.courseCode ?? '',
      grade: review.grade ?? '',
      year: review.year,
      comment: review.comment,
    })
    setSelectedTags(review.tags ?? [])
  }, [review, user, reset, navigate])

  function toggleTag(tag: string) {
    setSelectedTags(prev =>
      prev.includes(tag) ? prev.filter(t => t !== tag) : [...prev, tag]
    )
  }

  async function onSubmit(data: FormData) {
    if (!review) return
    if (data.qualityRating === 0) return setError('Kalite puanı seçiniz.')
    if (data.difficultyRating === 0) return setError('Zorluk puanı seçiniz.')
    setError('')
    setSubmitting(true)
    try {
      await reviewApi.update(reviewId, {
        ...data,
        tags: selectedTags,
        courseCode: data.courseCode || undefined,
        grade: data.grade || undefined,
      })
      await Promise.all([
        qc.invalidateQueries({ queryKey: ['reviews', review.professorId] }),
        qc.invalidateQueries({ queryKey: ['professor', review.professorId] }),
        qc.invalidateQueries({ queryKey: ['reviews', 'my'] }),
        qc.invalidateQueries({ queryKey: ['auth', 'me'] }),
        qc.invalidateQueries({ queryKey: ['review', reviewId] }),
      ])
      const destination = fromPath ?? `/professors/${review.professorId}`
      navigate(destination, { replace: true, state: destination === '/profile' ? { reviewUpdated: true } : undefined })
    } catch (e: any) {
      if (e.response?.status === 403) {
        setError('Bu yorumu düzenleme yetkiniz yok.')
      } else {
        setError(e.response?.data?.message || 'Bir hata oluştu.')
      }
    } finally {
      setSubmitting(false)
    }
  }

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

      <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-6">
        <div className="card p-5">
          <h2 className="font-semibold text-text mb-4">Puanlar</h2>
          <div className="flex flex-col sm:flex-row gap-6">
            <div>
              <label className="text-sm font-medium text-text-muted mb-2 block">
                Kalite <span className="text-danger">*</span>
              </label>
              <Controller
                name="qualityRating"
                control={control}
                render={({ field }) => (
                  <StarRating value={field.value} onChange={field.onChange} size={28} />
                )}
              />
            </div>
            <div>
              <label className="text-sm font-medium text-text-muted mb-2 block">
                Zorluk <span className="text-danger">*</span>
              </label>
              <Controller
                name="difficultyRating"
                control={control}
                render={({ field }) => (
                  <StarRating value={field.value} onChange={field.onChange} size={28} />
                )}
              />
            </div>
          </div>
        </div>

        <div className="card p-5">
          <h2 className="font-semibold text-text mb-4">Genel sorular</h2>
          <div className="flex flex-col gap-4">
            <label className="flex items-center justify-between">
              <span className="text-sm text-text">Bu hocayı tekrar alır mısın?</span>
              <div className="flex gap-2">
                {[true, false].map(val => (
                  <Controller key={String(val)} name="wouldTakeAgain" control={control} render={({ field }) => (
                    <button
                      type="button"
                      onClick={() => field.onChange(val)}
                      className={clsx(
                        'px-4 py-1.5 rounded text-sm font-medium transition-colors border',
                        field.value === val
                          ? val ? 'bg-green-50 border-green-400 text-green-700' : 'bg-red-50 border-red-400 text-red-700'
                          : 'border-surface-border text-text-muted hover:border-text-light'
                      )}
                    >
                      {val ? 'Evet' : 'Hayır'}
                    </button>
                  )} />
                ))}
              </div>
            </label>
            <label className="flex items-center justify-between">
              <span className="text-sm text-text">Devam zorunlu muydu?</span>
              <div className="flex gap-2">
                {[true, false].map(val => (
                  <Controller key={String(val)} name="attendanceMandatory" control={control} render={({ field }) => (
                    <button
                      type="button"
                      onClick={() => field.onChange(val)}
                      className={clsx(
                        'px-4 py-1.5 rounded text-sm font-medium transition-colors border',
                        field.value === val
                          ? 'bg-primary-light border-primary text-primary'
                          : 'border-surface-border text-text-muted hover:border-text-light'
                      )}
                    >
                      {val ? 'Evet' : 'Hayır'}
                    </button>
                  )} />
                ))}
              </div>
            </label>
          </div>
        </div>

        <div className="card p-5">
          <h2 className="font-semibold text-text mb-4">Ders bilgisi (isteğe bağlı)</h2>
          <div className="grid sm:grid-cols-3 gap-3">
            <div>
              <label className="text-xs font-medium text-text-muted mb-1 block">Ders kodu</label>
              <input {...register('courseCode')} className="input" placeholder="BLM101" />
            </div>
            <div>
              <label className="text-xs font-medium text-text-muted mb-1 block">Not</label>
              <input {...register('grade')} className="input" placeholder="AA, BA..." />
            </div>
            <div>
              <label className="text-xs font-medium text-text-muted mb-1 block">Yıl</label>
              <input {...register('year', { valueAsNumber: true })} type="number" className="input" min={2000} max={2030} />
            </div>
          </div>
        </div>

        <div className="card p-5">
          <h2 className="font-semibold text-text mb-4">Etiketler</h2>
          <div className="flex flex-wrap gap-2">
            {REVIEW_TAGS.map(tag => (
              <button
                key={tag}
                type="button"
                onClick={() => toggleTag(tag)}
                className={clsx(
                  'px-3 py-1.5 rounded-full text-sm border transition-all',
                  selectedTags.includes(tag)
                    ? 'bg-primary border-primary text-white'
                    : 'border-surface-border text-text-muted hover:border-primary hover:text-primary'
                )}
              >
                {tag}
              </button>
            ))}
          </div>
        </div>

        <div className="card p-5">
          <h2 className="font-semibold text-text mb-3">
            Yorumun <span className="text-danger">*</span>
          </h2>
          <textarea
            {...register('comment', { required: true, minLength: 20 })}
            rows={5}
            className="input resize-none"
          />
          {errors.comment && (
            <p className="text-xs text-danger mt-1">En az 20 karakter yazınız.</p>
          )}
        </div>

        {error && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-3 text-sm text-red-700">
            {error}
          </div>
        )}

        <div className="flex gap-3">
          <button type="submit" disabled={submitting} className="btn-primary flex-1 justify-center py-3">
            {submitting ? 'Kaydediliyor...' : 'Değişiklikleri kaydet'}
          </button>
          <Link to={backTo} className="btn-outline px-6 py-3">
            İptal
          </Link>
        </div>
      </form>
    </div>
  )
}
