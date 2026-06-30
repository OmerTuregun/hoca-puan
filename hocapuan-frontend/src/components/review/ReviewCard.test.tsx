import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import ReviewCard from './ReviewCard'
import type { Review } from '../../services/api'
import { reviewApi } from '../../services/api'
import { useAuthStore } from '../../store/authStore'

vi.mock('../../services/api', () => ({
  reviewApi: {
    vote: vi.fn(),
    freshnessVote: vi.fn(),
    report: vi.fn(),
    delete: vi.fn(),
    update: vi.fn(),
  },
}))

function makeReview(overrides: Partial<Review> = {}): Review {
  return {
    id: 1,
    userId: 42,
    professorId: 10,
    professorFullName: 'Prof. Test',
    universityName: 'Test Üni',
    username: 'testuser',
    year: 2024,
    qualityRating: 4,
    difficultyRating: 3,
    wouldTakeAgain: true,
    attendanceMandatory: false,
    comment: 'Harika bir ders.',
    tags: ['İlham Verici'],
    status: 'Approved',
    thumbsUp: 2,
    thumbsDown: 0,
    createdAt: '2024-06-01T10:00:00Z',
    isFreshnessVotingOpen: false,
    isFlaggedAsOutdated: false,
    ...overrides,
  }
}

function resetAuth(overrides: Partial<ReturnType<typeof useAuthStore.getState>> = {}) {
  useAuthStore.setState({
    user: null,
    csrfToken: null,
    isLoggedIn: false,
    isAdmin: false,
    hasHydrated: true,
    ...overrides,
  })
}

function renderCard(review: Review) {
  return render(
    <MemoryRouter>
      <ReviewCard review={review} />
    </MemoryRouter>,
  )
}

describe('ReviewCard', () => {
  beforeEach(() => {
    resetAuth()
    vi.mocked(reviewApi.vote).mockReset()
    vi.mocked(reviewApi.freshnessVote).mockReset()
  })

  it('shows Edit and Delete for the review owner', () => {
    resetAuth({
      isLoggedIn: true,
      user: { id: 42, username: 'owner', email: 'o@uni.edu.tr', role: 'User', isEmailVerified: true },
    })

    renderCard(makeReview({ userId: 42 }))

    expect(screen.getByRole('button', { name: 'Yorumu düzenle' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Yorumu sil' })).toBeInTheDocument()
  })

  it('hides Edit and Delete for other users', () => {
    resetAuth({
      isLoggedIn: true,
      user: { id: 99, username: 'other', email: 'x@uni.edu.tr', role: 'User', isEmailVerified: true },
    })

    renderCard(makeReview({ userId: 42 }))

    expect(screen.queryByRole('button', { name: 'Yorumu düzenle' })).not.toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Yorumu sil' })).not.toBeInTheDocument()
  })

  it('shows Delete but not Edit for admin on another user review', () => {
    resetAuth({
      isLoggedIn: true,
      isAdmin: true,
      user: { id: 1, username: 'admin', email: 'a@uni.edu.tr', role: 'Admin', isEmailVerified: true },
    })

    renderCard(makeReview({ userId: 42 }))

    expect(screen.queryByRole('button', { name: 'Yorumu düzenle' })).not.toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Yorumu sil' })).toBeInTheDocument()
  })

  it('opens edit form when owner clicks Düzenle', async () => {
    const user = userEvent.setup()
    resetAuth({
      isLoggedIn: true,
      user: { id: 42, username: 'owner', email: 'o@uni.edu.tr', role: 'User', isEmailVerified: true },
    })

    renderCard(makeReview({ userId: 42 }))

    await user.click(screen.getByRole('button', { name: 'Yorumu düzenle' }))
    expect(screen.getByText('Yorumu düzenle')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Değişiklikleri kaydet/i })).toBeInTheDocument()
  })

  it('shows backend rate limit message when vote returns 429', async () => {
    const user = userEvent.setup()
    const rateLimitMessage = 'Çok hızlı oy veriyorsunuz, lütfen biraz bekleyin.'
    vi.mocked(reviewApi.vote).mockRejectedValue({
      response: { status: 429, data: { message: rateLimitMessage } },
    })
    resetAuth({
      isLoggedIn: true,
      user: { id: 99, username: 'voter', email: 'v@uni.edu.tr', role: 'User', isEmailVerified: true },
    })

    renderCard(makeReview({ userId: 42, thumbsUp: 2 }))

    await user.click(screen.getByRole('button', { name: '2' }))

    expect(await screen.findByText(rateLimitMessage)).toBeInTheDocument()
    expect(screen.queryByText('Oy kaydedilemedi.')).not.toBeInTheDocument()
  })
})
