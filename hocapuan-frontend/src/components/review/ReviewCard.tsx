import { ThumbsUp, ThumbsDown, Trash2 } from 'lucide-react'
import type { Review } from '../../services/api'
import { reviewApi } from '../../services/api'
import { useAuthStore } from '../../store/authStore'
import RatingBadge from '../ui/RatingBadge'
import { useState } from 'react'
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

interface Props {
  review: Review
  onDelete?: () => void
  onVote?: () => void
}

export default function ReviewCard({ review: r, onDelete, onVote }: Props) {
  const { user, isLoggedIn } = useAuthStore()
  const [votes, setVotes] = useState({ up: r.thumbsUp, down: r.thumbsDown })
  const [userVote, setUserVote] = useState<boolean | null | undefined>(r.currentUserVote)
  const [voting, setVoting] = useState(false)

  async function handleVote(isUpvote: boolean) {
    if (!isLoggedIn || voting) return
    setVoting(true)
    try {
      const res = await reviewApi.vote(r.id, isUpvote)
      setVotes({ up: res.thumbsUp, down: res.thumbsDown })
      setUserVote(res.userVote)
      onVote?.()
    } finally {
      setVoting(false)
    }
  }

  const canDelete = user?.id === r.id || false

  return (
    <div className="card p-5 animate-fadeUp">
      {/* Üst satır */}
      <div className="flex items-start justify-between gap-3">
        <div className="flex gap-3">
          <RatingBadge value={r.qualityRating} size="sm" label="Kalite" />
          <RatingBadge value={r.difficultyRating} size="sm" label="Zorluk" />
        </div>
        <div className="text-right">
          <p className="text-sm font-semibold text-text">{r.username}</p>
          <p className="text-xs text-text-muted">
            {r.courseCode ? `${r.courseCode} · ` : ''}{r.year}
          </p>
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
      <div className="mt-4 flex items-center justify-between">
        <div className="flex items-center gap-1 text-xs text-text-muted">
          {r.wouldTakeAgain
            ? <span className="text-green-600 font-medium">✓ Tekrar alır</span>
            : <span className="text-red-500 font-medium">✗ Tekrar almaz</span>}
          {r.attendanceMandatory && <span className="ml-2">· Devam zorunlu</span>}
          {r.grade && <span className="ml-2">· Not: {r.grade}</span>}
        </div>

        <div className="flex items-center gap-2">
          {/* Oylama */}
          <button
            onClick={() => handleVote(true)}
            disabled={!isLoggedIn || voting}
            className={clsx(
              'flex items-center gap-1 px-2 py-1 rounded text-xs transition-colors',
              userVote === true
                ? 'bg-green-50 text-green-700'
                : 'hover:bg-surface-alt text-text-muted',
              !isLoggedIn && 'opacity-50 cursor-default'
            )}
          >
            <ThumbsUp className="w-3.5 h-3.5" />
            {votes.up}
          </button>
          <button
            onClick={() => handleVote(false)}
            disabled={!isLoggedIn || voting}
            className={clsx(
              'flex items-center gap-1 px-2 py-1 rounded text-xs transition-colors',
              userVote === false
                ? 'bg-red-50 text-red-700'
                : 'hover:bg-surface-alt text-text-muted',
              !isLoggedIn && 'opacity-50 cursor-default'
            )}
          >
            <ThumbsDown className="w-3.5 h-3.5" />
            {votes.down}
          </button>

          {canDelete && (
            <button
              onClick={onDelete}
              className="p-1 rounded hover:bg-red-50 text-text-light hover:text-red-500 transition-colors"
            >
              <Trash2 className="w-3.5 h-3.5" />
            </button>
          )}
        </div>
      </div>
    </div>
  )
}
