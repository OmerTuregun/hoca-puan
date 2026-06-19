import clsx from 'clsx'
import type { ReviewSubmitErrorKind } from '../../utils/reviewSubmitError'

interface ReviewFormErrorAlertProps {
  message: string
  kind: ReviewSubmitErrorKind
}

export default function ReviewFormErrorAlert({ message, kind }: ReviewFormErrorAlertProps) {
  const title =
    kind === 'moderation'
      ? 'Yorumunuz onaylanmadı'
      : kind === 'forbidden'
        ? 'İşlem reddedildi'
        : 'Gönderim başarısız'

  return (
    <div
      role="alert"
      className={clsx(
        'rounded-lg p-3 text-sm border',
        kind === 'general'
          ? 'bg-red-50 border-red-200 text-red-700'
          : 'bg-red-50 border-red-300 text-red-800'
      )}
    >
      <p className="font-medium mb-1">{title}</p>
      <p>{message}</p>
    </div>
  )
}
