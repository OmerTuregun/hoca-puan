import Spinner from './Spinner'

interface ReviewSubmitOverlayProps {
  message?: string
  detail?: string
}

export default function ReviewSubmitOverlay({
  message = 'Yorumunuz inceleniyor',
  detail = 'İçerik kontrolü yapılıyor, lütfen bekleyin.',
}: ReviewSubmitOverlayProps) {
  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 px-4"
      role="alertdialog"
      aria-modal="true"
      aria-busy="true"
      aria-labelledby="review-submit-title"
      aria-describedby="review-submit-detail"
    >
      <div className="card max-w-sm w-full p-8 text-center shadow-xl">
        <Spinner className="py-0 mb-5" />
        <p id="review-submit-title" className="font-semibold text-text text-lg">
          {message}
        </p>
        <p id="review-submit-detail" className="text-sm text-text-muted mt-2">
          {detail}
        </p>
      </div>
    </div>
  )
}
