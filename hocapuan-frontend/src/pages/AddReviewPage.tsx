import { useParams, useNavigate, Link } from 'react-router-dom'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useForm, Controller } from 'react-hook-form'
import { professorApi, reviewApi } from '../services/api'
import { useAuthStore } from '../store/authStore'
import StarRating from '../components/ui/StarRating'
import Spinner from '../components/ui/Spinner'
import ReviewSubmitOverlay from '../components/ui/ReviewSubmitOverlay'
import ReviewFormErrorAlert from '../components/review/ReviewFormErrorAlert'
import ReviewSubmitButton from '../components/review/ReviewSubmitButton'
import { parseReviewSubmitError, type ReviewSubmitErrorKind } from '../utils/reviewSubmitError'
import { useState, useEffect } from 'react'
import clsx from 'clsx'
import { REVIEW_TAGS } from '../constants/reviewTags'
import { useCommentModerationHint } from '../hooks/useCommentModerationHint'

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

export default function AddReviewPage() {
  const { id } = useParams<{ id: string }>()
  const profId = Number(id)
  const navigate = useNavigate()
  const qc = useQueryClient()
  const { isLoggedIn, hasHydrated } = useAuthStore()
  const [selectedTags, setSelectedTags] = useState<string[]>([])
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')
  const [errorKind, setErrorKind] = useState<ReviewSubmitErrorKind>('general')

  useEffect(() => {
    if (!hasHydrated) return
    if (!isLoggedIn) {
      navigate('/login', { replace: true, state: { from: `/professors/${profId}/review` } })
    }
  }, [hasHydrated, isLoggedIn, navigate, profId])

  const { data: professor, isLoading } = useQuery({
    queryKey: ['professor', profId],
    queryFn:  () => professorApi.get(profId),
  })

  const { register, control, handleSubmit, watch, formState: { errors } } = useForm<FormData>({
    defaultValues: {
      qualityRating: 0,
      difficultyRating: 0,
      wouldTakeAgain: true,
      attendanceMandatory: false,
      year: new Date().getFullYear(),
    }
  })

  const commentValue = watch('comment') ?? ''
  const moderationHint = useCommentModerationHint(commentValue)

  function toggleTag(tag: string) {
    if (submitting) return
    setSelectedTags(prev =>
      prev.includes(tag) ? prev.filter(t => t !== tag) : [...prev, tag]
    )
  }

  async function onSubmit(data: FormData) {
    if (data.qualityRating === 0) {
      setErrorKind('general')
      return setError('Kalite puanı seçiniz.')
    }
    if (data.difficultyRating === 0) {
      setErrorKind('general')
      return setError('Zorluk puanı seçiniz.')
    }
    setError('')
    setSubmitting(true)
    try {
      const created = await reviewApi.create({
        professorId: profId,
        ...data,
        tags: selectedTags,
        courseCode: data.courseCode || undefined,
        grade: data.grade || undefined,
      })
      await Promise.all([
        qc.invalidateQueries({ queryKey: ['reviews', profId] }),
        qc.invalidateQueries({ queryKey: ['professor', profId] }),
        qc.invalidateQueries({ queryKey: ['reviews', 'my'] }),
      ])
      navigate(`/professors/${profId}`, {
        replace: true,
        state: {
          reviewSuccess: true,
          reviewPending: created.status === 'Pending',
          reviewInfoMessage: created.infoMessage,
        },
      })
    } catch (e: unknown) {
      const parsed = parseReviewSubmitError(e)
      setErrorKind(parsed.kind)
      setError(parsed.message)
    } finally {
      setSubmitting(false)
    }
  }

  if (!hasHydrated || !isLoggedIn) return <Spinner />
  if (isLoading) return <Spinner />
  if (!professor) return <div className="text-center py-20">Hoca bulunamadı.</div>

  return (
    <div className="max-w-2xl mx-auto px-4 py-8">
      {submitting && <ReviewSubmitOverlay />}
      <Link to={`/professors/${profId}`} className="text-sm text-text-muted hover:text-primary mb-4 inline-block">
        ← {professor.title} {professor.firstName} {professor.lastName}
      </Link>
      <h1 className="font-display text-3xl text-text mb-6">Yorum yaz</h1>

      <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-6">
        <fieldset
          disabled={submitting}
          className="flex flex-col gap-6 border-0 p-0 m-0 min-w-0 disabled:opacity-70"
        >
        {/* Puanlar */}
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
                  <StarRating
                    value={field.value}
                    onChange={field.onChange}
                    readonly={submitting}
                    size={28}
                  />
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
                  <StarRating
                    value={field.value}
                    onChange={field.onChange}
                    readonly={submitting}
                    size={28}
                  />
                )}
              />
            </div>
          </div>
        </div>

        {/* Evet/Hayır */}
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

        {/* Ders bilgisi */}
        <div className="card p-5">
          <h2 className="font-semibold text-text mb-4">Ders bilgisi (isteğe bağlı)</h2>
          <div className="grid sm:grid-cols-3 gap-3">
            <div>
              <label className="text-xs font-medium text-text-muted mb-1 block">Ders kodu</label>
              <input {...register('courseCode')} className="input" placeholder="BLM101" disabled={submitting} />
            </div>
            <div>
              <label className="text-xs font-medium text-text-muted mb-1 block">Not</label>
              <input {...register('grade')} className="input" placeholder="AA, BA..." disabled={submitting} />
            </div>
            <div>
              <label className="text-xs font-medium text-text-muted mb-1 block">Yıl</label>
              <input {...register('year', { valueAsNumber: true })} type="number" className="input" min={2000} max={2030} disabled={submitting} />
            </div>
          </div>
        </div>

        {/* Etiketler */}
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

        {/* Yorum */}
        <div className="card p-5">
          <h2 className="font-semibold text-text mb-3">
            Yorumun <span className="text-danger">*</span>
          </h2>
          <textarea
            {...register('comment', { required: true, minLength: 20 })}
            rows={5}
            placeholder="Hoca hakkındaki deneyimini paylaş. Sınav şekli, ders anlatımı, iletişim..."
            className="input resize-none"
            disabled={submitting}
          />
          {errors.comment && (
            <p className="text-xs text-danger mt-1">En az 20 karakter yazınız.</p>
          )}
          {moderationHint && (
            <p className="text-xs text-amber-700 bg-amber-50 border border-amber-200 rounded-lg p-2 mt-2">
              ⚠️ Yorumunuz uygunsuz içerik barındırabilir, gönderildiğinde reddedilebilir.
            </p>
          )}
        </div>

        </fieldset>

        {error && <ReviewFormErrorAlert message={error} kind={errorKind} />}

        <div className="flex gap-3">
          <ReviewSubmitButton submitting={submitting} idleLabel="Yorumu gönder" />
          <Link
            to={`/professors/${profId}`}
            className={clsx('btn-outline px-6 py-3', submitting && 'pointer-events-none opacity-50')}
            tabIndex={submitting ? -1 : undefined}
            aria-disabled={submitting}
          >
            İptal
          </Link>
        </div>
      </form>
    </div>
  )
}
