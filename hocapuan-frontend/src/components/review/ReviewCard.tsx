import { ThumbsUp, ThumbsDown, Trash2, Pencil, Flag } from 'lucide-react'
import { Link } from 'react-router-dom'
import type { Review } from '../../services/api'
import { reviewApi } from '../../services/api'
import { useAuthStore } from '../../store/authStore'
import RatingBadge from '../ui/RatingBadge'
import ReviewEditForm from './ReviewEditForm'
import ConfirmModal from '../ui/ConfirmModal'
import { buildDeleteConfirmMessage, parseReviewDeleteError } from '../../utils/reviewDeleteError'
import { useState, useEffect } from 'react'
import clsx from 'clsx'

type FreshnessState = {
  isFlaggedAsOutdated: boolean
  freshnessStillValidPercentage?: number | null
  currentUserFreshnessVote?: boolean | null
}

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
  onDeleted?: () => void
  onVote?: () => void
  onUpdate?: (updated: Review) => void
}

export default function ReviewCard({ review: r, onDeleted, onVote, onUpdate }: Props) {
  const { user, isLoggedIn, isAdmin } = useAuthStore()
  const [votes, setVotes] = useState({ up: r.thumbsUp, down: r.thumbsDown })
  const [userVote, setUserVote] = useState<boolean | null | undefined>(r.currentUserVote)
  const [voting, setVoting] = useState(false)
  const [voteError, setVoteError] = useState('')
  const [isEditing, setIsEditing] = useState(false)
  const [deleting, setDeleting] = useState(false)
  const [deleteError, setDeleteError] = useState('')
  const [showDeleteModal, setShowDeleteModal] = useState(false)
  const [reporting, setReporting] = useState(false)
  const [reportMessage, setReportMessage] = useState('')
  const [reportError, setReportError] = useState('')
  const [showReportModal, setShowReportModal] = useState(false)
  const [freshness, setFreshness] = useState<FreshnessState>({
    isFlaggedAsOutdated: r.isFlaggedAsOutdated,
    freshnessStillValidPercentage: r.freshnessStillValidPercentage,
    currentUserFreshnessVote: r.currentUserFreshnessVote,
  })
  const [freshnessVoting, setFreshnessVoting] = useState(false)

  useEffect(() => {
    setVotes({ up: r.thumbsUp, down: r.thumbsDown })
    setUserVote(r.currentUserVote)
    setFreshness({
      isFlaggedAsOutdated: r.isFlaggedAsOutdated,
      freshnessStillValidPercentage: r.freshnessStillValidPercentage,
      currentUserFreshnessVote: r.currentUserFreshnessVote,
    })
  }, [r.id, r.thumbsUp, r.thumbsDown, r.currentUserVote, r.isFlaggedAsOutdated, r.freshnessStillValidPercentage, r.currentUserFreshnessVote])

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

  async function handleFreshnessVote(isStillValid: boolean) {
    if (!isLoggedIn || freshnessVoting) return
    setFreshnessVoting(true)
    try {
      const res = await reviewApi.freshnessVote(r.id, isStillValid)
      setFreshness({
        isFlaggedAsOutdated: res.isFlaggedAsOutdated,
        freshnessStillValidPercentage: res.freshnessStillValidPercentage,
        currentUserFreshnessVote: res.currentUserFreshnessVote,
      })
      onVote?.()
    } catch {
      // silently ignore; voting may not be open yet
    } finally {
      setFreshnessVoting(false)
    }
  }

  const isOwner = user?.id === r.userId
  const canDelete = isLoggedIn && (isOwner || isAdmin)
  const canEdit = isOwner
  const canReport = isLoggedIn && !isOwner
  const canFreshnessVote =
    r.isFreshnessVotingOpen &&
    isLoggedIn &&
    !isOwner &&
    freshness.currentUserFreshnessVote == null

  async function confirmReport() {
    setReporting(true)
    setReportError('')
    setReportMessage('')
    try {
      const res = await reviewApi.report(r.id)
      setShowReportModal(false)
      setReportMessage(res.message || 'Bildiriminiz alındı.')
    } catch (error: unknown) {
      const message =
        (error as { response?: { data?: { message?: string } } })?.response?.data?.message
        ?? 'Bildirim gönderilemedi.'
      setReportError(message)
      setShowReportModal(false)
    } finally {
      setReporting(false)
    }
  }

  async function confirmDelete() {
    setDeleting(true)
    setDeleteError('')
    try {
      await reviewApi.delete(r.id)
      setShowDeleteModal(false)
      onDeleted?.()
    } catch (error: unknown) {
      setDeleteError(parseReviewDeleteError(error))
      setShowDeleteModal(false)
    } finally {
      setDeleting(false)
    }
  }

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
    <>
      <ConfirmModal
        open={showDeleteModal}
        title="Yorumu sil"
        message={buildDeleteConfirmMessage(r.username, isOwner, isAdmin)}
        loading={deleting}
        onCancel={() => !deleting && setShowDeleteModal(false)}
        onConfirm={() => void confirmDelete()}
      />
      <ConfirmModal
        open={showReportModal}
        title="Yorumu bildir"
        message="Bu yorumu uygunsuz olarak bildirmek istediğinize emin misiniz?"
        confirmLabel="Bildir"
        loading={reporting}
        onCancel={() => !reporting && setShowReportModal(false)}
        onConfirm={() => void confirmReport()}
      />
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
          {r.courseCode && (
            <p className="text-xs text-text-light mt-0.5">
              {r.courseCode}{r.year ? ` · ${r.year}` : ''}
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

      {/* Güncellik uyarısı */}
      {freshness.isFlaggedAsOutdated && (
        <div className="mt-3 flex items-center gap-1.5 rounded-md bg-amber-50 border border-amber-200 px-3 py-2 text-xs text-amber-700">
          <span>⚠️</span>
          <span>Bazı kullanıcılar bu bilginin güncelliğini sorguluyor</span>
          {freshness.freshnessStillValidPercentage != null && (
            <span className="ml-auto font-medium">
              %{Math.round(freshness.freshnessStillValidPercentage)} güncel
            </span>
          )}
        </div>
      )}

      {/* Güncellik oylaması */}
      {canFreshnessVote && (
        <div className="mt-3 flex flex-wrap items-center gap-2 rounded-md bg-surface border border-surface-border px-3 py-2 text-xs text-text-muted">
          <span className="font-medium text-text">Bu bilgi hâlâ geçerli mi?</span>
          <button
            type="button"
            onClick={() => void handleFreshnessVote(true)}
            disabled={freshnessVoting}
            className="flex items-center gap-1 px-2 py-1 rounded bg-green-50 text-green-700 hover:bg-green-100 transition-colors disabled:opacity-50"
          >
            Evet
          </button>
          <button
            type="button"
            onClick={() => void handleFreshnessVote(false)}
            disabled={freshnessVoting}
            className="flex items-center gap-1 px-2 py-1 rounded bg-red-50 text-red-600 hover:bg-red-100 transition-colors disabled:opacity-50"
          >
            Hayır
          </button>
        </div>
      )}

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
                    'flex items-center gap-1 px-2 py-1 rounded text-xs transition-colors min-h-[44px] min-w-[44px] justify-center -my-[10px]',
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
                    'flex items-center gap-1 px-2 py-1 rounded text-xs transition-colors min-h-[44px] min-w-[44px] justify-center -my-[10px]',
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
                className="touch-link text-sm text-primary hover:underline"
              >
                Oy vermek için giriş yap
              </Link>
            )}

            {canDelete && (
              <button
                type="button"
                onClick={() => setShowDeleteModal(true)}
                disabled={deleting}
                className={clsx(
                  'p-1 rounded hover:bg-red-50 text-text-light hover:text-red-500 transition-colors',
                  deleting && 'opacity-50 cursor-not-allowed'
                )}
                aria-label="Yorumu sil"
              >
                <Trash2 className="w-3.5 h-3.5" />
              </button>
            )}

            {canReport && (
              <button
                type="button"
                onClick={() => setShowReportModal(true)}
                disabled={reporting}
                className={clsx(
                  'p-1 rounded hover:bg-amber-50 text-text-light hover:text-amber-600 transition-colors',
                  reporting && 'opacity-50 cursor-not-allowed'
                )}
                aria-label="Yorumu bildir"
              >
                <Flag className="w-3.5 h-3.5" />
              </button>
            )}
          </div>
          {(voteError || deleteError || reportError || reportMessage) && (
            <p className={clsx(
              'text-xs',
              reportMessage ? 'text-green-600' : 'text-danger'
            )}>
              {reportMessage || deleteError || reportError || voteError}
            </p>
          )}
        </div>
      </div>
    </div>
    </>
  )
}
