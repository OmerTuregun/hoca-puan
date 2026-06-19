import { useState, useEffect } from 'react'
import { useForm, Controller } from 'react-hook-form'
import clsx from 'clsx'
import type { Review } from '../../services/api'
import { reviewApi } from '../../services/api'
import StarRating from '../ui/StarRating'
import ReviewSubmitOverlay from '../ui/ReviewSubmitOverlay'
import ReviewFormErrorAlert from './ReviewFormErrorAlert'
import ReviewSubmitButton from './ReviewSubmitButton'
import { parseReviewSubmitError, type ReviewSubmitErrorKind } from '../../utils/reviewSubmitError'
import { REVIEW_TAGS } from '../../constants/reviewTags'
import { useCommentModerationHint } from '../../hooks/useCommentModerationHint'

export interface ReviewFormData {
  qualityRating: number
  difficultyRating: number
  wouldTakeAgain: boolean
  attendanceMandatory: boolean
  courseCode: string
  grade: string
  year: number
  comment: string
}

interface Props {
  review: Review
  onSuccess: (updated: Review) => void
  onCancel: () => void
  compact?: boolean
}

export default function ReviewEditForm({ review, onSuccess, onCancel, compact }: Props) {
  const [selectedTags, setSelectedTags] = useState<string[]>(review.tags ?? [])
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')
  const [errorKind, setErrorKind] = useState<ReviewSubmitErrorKind>('general')

  const { register, control, handleSubmit, watch, reset, formState: { errors } } = useForm<ReviewFormData>()
  const commentValue = watch('comment') ?? ''
  const moderationHint = useCommentModerationHint(commentValue)
  const starSize = compact ? 22 : 28

  useEffect(() => {
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
  }, [review, reset])

  function toggleTag(tag: string) {
    if (submitting) return
    setSelectedTags(prev =>
      prev.includes(tag) ? prev.filter(t => t !== tag) : [...prev, tag]
    )
  }

  async function onSubmit(data: ReviewFormData) {
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
      const updated = await reviewApi.update(review.id, {
        ...data,
        tags: selectedTags,
        courseCode: data.courseCode || undefined,
        grade: data.grade || undefined,
      })
      onSuccess(updated)
    } catch (e: unknown) {
      const parsed = parseReviewSubmitError(e)
      setErrorKind(parsed.kind)
      setError(parsed.message)
    } finally {
      setSubmitting(false)
    }
  }

  const sectionClass = compact ? 'border border-surface-border rounded-lg p-4' : 'card p-5'

  return (
    <>
      {submitting && (
        <ReviewSubmitOverlay detail="Değişiklikleriniz kaydediliyor ve inceleniyor, lütfen bekleyin." />
      )}
      <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
        <fieldset
          disabled={submitting}
          className="flex flex-col gap-4 border-0 p-0 m-0 min-w-0 disabled:opacity-70"
        >
          <div className={sectionClass}>
            {!compact && <h2 className="font-semibold text-text mb-4">Puanlar</h2>}
            <div className="flex flex-col sm:flex-row gap-4 sm:gap-6">
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
                      size={starSize}
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
                      size={starSize}
                    />
                  )}
                />
              </div>
            </div>
          </div>

          <div className={sectionClass}>
            {!compact && <h2 className="font-semibold text-text mb-4">Genel sorular</h2>}
            <div className="flex flex-col gap-3">
              <label className="flex items-center justify-between gap-3">
                <span className="text-sm text-text">Bu hocayı tekrar alır mısın?</span>
                <div className="flex gap-2">
                  {[true, false].map(val => (
                    <Controller key={String(val)} name="wouldTakeAgain" control={control} render={({ field }) => (
                      <button
                        type="button"
                        onClick={() => field.onChange(val)}
                        className={clsx(
                          'px-3 py-1.5 rounded text-sm font-medium transition-colors border',
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
              <label className="flex items-center justify-between gap-3">
                <span className="text-sm text-text">Devam zorunlu muydu?</span>
                <div className="flex gap-2">
                  {[true, false].map(val => (
                    <Controller key={String(val)} name="attendanceMandatory" control={control} render={({ field }) => (
                      <button
                        type="button"
                        onClick={() => field.onChange(val)}
                        className={clsx(
                          'px-3 py-1.5 rounded text-sm font-medium transition-colors border',
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

          <div className={sectionClass}>
            {!compact && <h2 className="font-semibold text-text mb-4">Ders bilgisi (isteğe bağlı)</h2>}
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

          <div className={sectionClass}>
            {!compact && <h2 className="font-semibold text-text mb-4">Etiketler</h2>}
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

          <div className={sectionClass}>
            {!compact && (
              <h2 className="font-semibold text-text mb-3">
                Yorumun <span className="text-danger">*</span>
              </h2>
            )}
            <textarea
              {...register('comment', { required: true, minLength: 20 })}
              rows={compact ? 4 : 5}
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
          <ReviewSubmitButton submitting={submitting} idleLabel="Değişiklikleri kaydet" />
          <button
            type="button"
            onClick={onCancel}
            disabled={submitting}
            className="btn-outline px-6 py-3 disabled:opacity-50"
          >
            İptal
          </button>
        </div>
      </form>
    </>
  )
}
