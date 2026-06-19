import { ThumbsUp, ThumbsDown, Trash2, Pencil } from 'lucide-react'
import { Link } from 'react-router-dom'
import type { Review } from '../../services/api'
import { reviewApi } from '../../services/api'
import { useAuthStore } from '../../store/authStore'
import RatingBadge from '../ui/RatingBadge'
import ReviewEditForm from './ReviewEditForm'
import { useState, useEffect } from 'react'
import clsx from 'clsx'

const TAGS: Record<string, string> = {
  'Çok Ödev Verir':        '#EEF1FD',
  'Az Ödev Verir':          '#F0FDF4',
  'Proje Odaklı':           '#FFF7ED',
  'Sınava Dayalı':          '#FFF1F2',
  'Devama Dikkat Eder':     '#FEFCE8',
  'Devam Şart Değil':       '#F0FDF4',
  'İlham Verici':           '#EEF1FD',
  'Yardımsever':            '#F0FDF4',
  'Ulaşılması Zor':         '#FFF1F2',
  'Notlar Kolay Anlaşılır': '#EEF1FD',
}

function formatReviewDate(iso: string) {
  return new Date(iso).toLocaleDateString('tr-TR', {
    day: 'numeric',
    month: 'long',
    year: 'numeric',
  })
}

interface Props {
  review: Review
  onDelete?: () => void
  onVote?: () => void
  onUpdate?: (updated: Review) => void
}

export default function ReviewCard({ review: r, onDelete, onVote, onUpdate }: Props) {
  const { user, isLoggedIn } = useAuthStore()
  const [votes, setVotes] = useState({ up: r.thumbsUp, down: r.thumbsDown })
  const [userVote, setUserVote] = useState<boolean | null | undefined>(r.currentUserVote)
  const [voting, setVoting] = useState(false)
  const [voteError, setVoteError] = useState('')
  const [isEditing, setIsEditing] = useState(false)

  useEffect(() => {
    setVotes({ up: r.thumbsUp, down: r.thumbsDown })
    setUserVote(r.currentUserVote)
  }, [r.id, r.thumbsUp, r.thumbsDown, r.currentUserVote])

  async function handleVote(isUpvote: boolean) {
    if (!isLoggedIn || voting) return
    setVoting(true)
    setVoteError('')
    try {
      const res = await reviewApi.vote(r.id, isUpvote)
      setVotes({ up: res.thumbsUp, down: res.thumbsDown })
      setUserVote(res.userVote ?? undefined)
      onVote?.()
    } catch {
      setVoteError('Oy kaydedilemedi.')
    } finally {
      setVoting(false)
    }
  }

  const isOwner = user?.id === r.userId
  const canDelete = isOwner && onDelete
  const canEdit = isOwner

  if (isEditing) {
    return (
      <div className="card p-5 animate-fadeUp">
        <div className="flex items-center justify-between mb-4">
          <p className="text-sm font-semibold text-text">Yorumu düzenle</p>
          <p className="text-xs text-text-muted">{formatReviewDate(r.createdAt)}</p>
        </div>
        <ReviewEditForm
          review={r}
          compact
          onCancel={() => setIsEditing(false)}
          onSuccess={(updated) => {
            setIsEditing(false)
            onUpdate?.(updated)
          }}
        />
      </div>
    )
  }

  return (
    <div className="card p-5 animate-fadeUp">
      {/* Üst satır */}
      <div className="flex items-start justify-between gap-3">
        <div className="flex gap-3">
          <RatingBadge value={r.qualityRating} size="sm" label="Kalite" />
          <RatingBadge value={r.difficultyRating} size="sm" label="Zorluk" />
        </div>
        <div className="text-right">
          <div className="flex items-center justify-end gap-2">
            {canEdit && (
              <button
                type="button"
                onClick={() => setIsEditing(true)}
                className="inline-flex items-center gap-1 text-xs text-primary hover:text-primary-dark transition-colors"
                aria-label="Yorumu düzenle"
              >
                <Pencil className="w-3.5 h-3.5" />
                Düzenle
              </button>
            )}
            <p className="text-sm font-semibold text-text">{r.username}</p>
          </div>
          <p className="text-xs text-text-muted">{formatReviewDate(r.createdAt)}</p>
          {(r.courseCode || r.year) && (
            <p className="text-xs text-text-light mt-0.5">
              {r.courseCode ? `${r.courseCode} · ` : ''}{r.year}
            </p>
          )}
        </div>
      </div>

      {/* Etiketler */}
      {r.tags.length > 0 && (
        <div className="mt-3 flex flex-wrap gap-1.5">
          {r.tags.map(tag => (
            <span
              key={tag}
              className="px-2 py-0.5 rounded-full text-xs font-medium text-text border border-surface-border"
              style={{ background: TAGS[tag] || '#F4F6FA' }}
            >
              {tag}
            </span>
          ))}
        </div>
      )}

      {/* Yorum */}
      <p className="mt-3 text-sm text-text leading-relaxed">{r.comment}</p>

      {/* Alt satır */}
      <div className="mt-4 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
        <div className="flex flex-wrap items-center gap-x-3 gap-y-1 text-xs text-text-muted">
          {r.wouldTakeAgain
            ? <span className="text-green-600 font-medium">✓ Tekrar alır</span>
            : <span className="text-red-500 font-medium">✗ Tekrar almaz</span>}
          <span>
            Devam zorunlu: <span className="font-medium text-text">{r.attendanceMandatory ? 'Evet' : 'Hayır'}</span>
          </span>
          {r.grade && <span>Not: <span className="font-medium text-text">{r.grade}</span></span>}
        </div>

        <div className="flex flex-col items-end gap-1">
          <div className="flex items-center gap-2">
            {isLoggedIn ? (
              <>
                <button
                  type="button"
                  onClick={() => handleVote(true)}
                  disabled={voting}
                  className={clsx(
                    'flex items-center gap-1 px-2 py-1 rounded text-xs transition-colors',
                    userVote === true
                      ? 'bg-green-50 text-green-700 ring-1 ring-green-200'
                      : 'hover:bg-green-50 hover:text-green-700 text-text-muted'
                  )}
                >
                  <ThumbsUp className="w-3.5 h-3.5" />
                  {votes.up}
                </button>
                <button
                  type="button"
                  onClick={() => handleVote(false)}
                  disabled={voting}
                  className={clsx(
                    'flex items-center gap-1 px-2 py-1 rounded text-xs transition-colors',
                    userVote === false
                      ? 'bg-red-50 text-red-700 ring-1 ring-red-200'
                      : 'hover:bg-red-50 hover:text-red-700 text-text-muted'
                  )}
                >
                  <ThumbsDown className="w-3.5 h-3.5" />
                  {votes.down}
                </button>
              </>
            ) : (
              <Link
                to="/login"
                state={{ from: window.location.pathname }}
                className="text-xs text-primary hover:underline"
              >
                Oy vermek için giriş yap
              </Link>
            )}

            {canDelete && (
              <button
                type="button"
                onClick={onDelete}
                className="p-1 rounded hover:bg-red-50 text-text-light hover:text-red-500 transition-colors"
                aria-label="Yorumu sil"
              >
                <Trash2 className="w-3.5 h-3.5" />
              </button>
            )}
          </div>
          {voteError && <p className="text-xs text-danger">{voteError}</p>}
        </div>
      </div>
    </div>
  )
}
