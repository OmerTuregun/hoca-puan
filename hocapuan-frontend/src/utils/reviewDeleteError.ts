import axios from 'axios'

export function parseReviewDeleteError(error: unknown): string {
  if (axios.isAxiosError(error)) {
    const status = error.response?.status
    const message = error.response?.data?.message as string | undefined

    if (status === 403) return 'Bu yorumu silme yetkiniz yok.'
    if (status === 404) return 'Yorum bulunamadı.'
    if (message) return message
  }

  return 'Yorum silinemedi. Lütfen tekrar deneyin.'
}

export function buildDeleteConfirmMessage(
  reviewUsername: string,
  isOwner: boolean,
  isAdmin: boolean,
): string {
  if (isAdmin && !isOwner) {
    return `Bu yorum ${reviewUsername} tarafından yazılmış. Silmek istediğinize emin misiniz?`
  }
  return 'Bu yorumu silmek istediğinize emin misiniz?'
}
