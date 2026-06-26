import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import ReviewEditForm from './ReviewEditForm'
import type { Review } from '../../services/api'
import { reviewApi } from '../../services/api'

vi.mock('../../services/api', () => ({
  reviewApi: {
    update: vi.fn(),
  },
}))

vi.mock('../../hooks/useCommentModerationHint', () => ({
  useCommentModerationHint: () => false,
}))

const mockReview: Review = {
  id: 5,
  userId: 1,
  professorId: 10,
  professorFullName: 'Prof. Test',
  universityName: 'Test Üni',
  username: 'tester',
  courseCode: 'BLM101',
  grade: 'AA',
  year: 2023,
  qualityRating: 4,
  difficultyRating: 3,
  wouldTakeAgain: true,
  attendanceMandatory: true,
  comment: 'Bu ders çok faydalıydı ve öğretici.',
  tags: ['İlham Verici'],
  status: 'Approved',
  thumbsUp: 0,
  thumbsDown: 0,
  createdAt: '2024-01-15T12:00:00Z',
  isFreshnessVotingOpen: false,
  isFlaggedAsOutdated: false,
}

describe('ReviewEditForm', () => {
  const onSuccess = vi.fn()
  const onCancel = vi.fn()

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders form fields with review data', () => {
    render(
      <ReviewEditForm review={mockReview} onSuccess={onSuccess} onCancel={onCancel} compact />,
    )

    expect(screen.getByDisplayValue('BLM101')).toBeInTheDocument()
    expect(screen.getByDisplayValue('AA')).toBeInTheDocument()
    expect(screen.getByDisplayValue('2023')).toBeInTheDocument()
    expect(screen.getByDisplayValue(mockReview.comment)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Değişiklikleri kaydet/i })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'İptal' })).toBeInTheDocument()
  })

  it('submits updated data via reviewApi.update and calls onSuccess', async () => {
    const user = userEvent.setup()
    const updatedReview = { ...mockReview, comment: 'Güncellenmiş yorum metni burada.' }
    vi.mocked(reviewApi.update).mockResolvedValue(updatedReview)

    render(
      <ReviewEditForm review={mockReview} onSuccess={onSuccess} onCancel={onCancel} compact />,
    )

    const textarea = screen.getByDisplayValue(mockReview.comment)
    await user.clear(textarea)
    await user.type(textarea, 'Güncellenmiş yorum metni burada.')

    await user.click(screen.getByRole('button', { name: /Değişiklikleri kaydet/i }))

    await waitFor(() => {
      expect(reviewApi.update).toHaveBeenCalledWith(
        5,
        expect.objectContaining({
          comment: 'Güncellenmiş yorum metni burada.',
          qualityRating: 4,
          difficultyRating: 3,
          tags: ['İlham Verici'],
        }),
      )
    })

    await waitFor(() => {
      expect(onSuccess).toHaveBeenCalledWith(updatedReview)
    })
  })

  it('calls onCancel when İptal is clicked', async () => {
    const user = userEvent.setup()
    render(
      <ReviewEditForm review={mockReview} onSuccess={onSuccess} onCancel={onCancel} compact />,
    )

    await user.click(screen.getByRole('button', { name: 'İptal' }))
    expect(onCancel).toHaveBeenCalled()
  })
})
