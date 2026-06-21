import { useEffect, useId } from 'react'
import clsx from 'clsx'

interface Props {
  open: boolean
  title: string
  message: string
  confirmLabel?: string
  cancelLabel?: string
  loading?: boolean
  onConfirm: () => void
  onCancel: () => void
}

export default function ConfirmModal({
  open,
  title,
  message,
  confirmLabel = 'Sil',
  cancelLabel = 'İptal',
  loading = false,
  onConfirm,
  onCancel,
}: Props) {
  const titleId = useId()
  const messageId = useId()

  useEffect(() => {
    if (!open) return

    function onKeyDown(e: KeyboardEvent) {
      if (e.key === 'Escape' && !loading) onCancel()
    }

    document.addEventListener('keydown', onKeyDown)
    const prevOverflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'

    return () => {
      document.removeEventListener('keydown', onKeyDown)
      document.body.style.overflow = prevOverflow
    }
  }, [open, loading, onCancel])

  if (!open) return null

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 px-4"
      role="presentation"
      onClick={loading ? undefined : onCancel}
    >
      <div
        role="alertdialog"
        aria-modal="true"
        aria-labelledby={titleId}
        aria-describedby={messageId}
        className="card max-w-md w-full p-6 shadow-xl"
        onClick={e => e.stopPropagation()}
      >
        <h2 id={titleId} className="font-display text-xl text-text">
          {title}
        </h2>
        <p id={messageId} className="text-sm text-text-muted mt-2 leading-relaxed">
          {message}
        </p>

        <div className="flex flex-col-reverse sm:flex-row sm:justify-end gap-2 mt-6">
          <button
            type="button"
            onClick={onCancel}
            disabled={loading}
            className="btn-outline px-5 py-2.5 disabled:opacity-50"
          >
            {cancelLabel}
          </button>
          <button
            type="button"
            onClick={onConfirm}
            disabled={loading}
            className={clsx(
              'px-5 py-2.5 rounded-lg font-medium text-white transition-colors',
              'bg-red-600 hover:bg-red-700 disabled:opacity-50'
            )}
          >
            {loading ? 'Siliniyor...' : confirmLabel}
          </button>
        </div>
      </div>
    </div>
  )
}
