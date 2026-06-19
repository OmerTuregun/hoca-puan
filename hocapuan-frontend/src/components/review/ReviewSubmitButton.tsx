interface ReviewSubmitButtonProps {
  submitting: boolean
  idleLabel: string
}

export default function ReviewSubmitButton({ submitting, idleLabel }: ReviewSubmitButtonProps) {
  return (
    <button
      type="submit"
      disabled={submitting}
      aria-busy={submitting}
      className="btn-primary flex-1 justify-center py-3 inline-flex items-center gap-2"
    >
      {submitting && (
        <span
          className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin shrink-0"
          aria-hidden
        />
      )}
      {submitting ? 'Yorumunuz inceleniyor...' : idleLabel}
    </button>
  )
}
