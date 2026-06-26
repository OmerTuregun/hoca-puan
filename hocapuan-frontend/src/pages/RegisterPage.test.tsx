import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import RegisterPage from './RegisterPage'
import { renderWithProviders } from '../test/testUtils'

vi.mock('../services/api', () => ({
  authApi: {
    register: vi.fn(),
  },
  universityApi: {
    list: vi.fn().mockResolvedValue([{ id: 1, name: 'Test Üniversitesi' }]),
  },
}))

describe('RegisterPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('shows .edu.tr warning for non-university email', async () => {
    const user = userEvent.setup({ delay: null })
    renderWithProviders(<RegisterPage />)

    const emailInput = screen.getByPlaceholderText('ornek@uni.edu.tr')
    await user.type(emailInput, 'user@gmail.com')

    await waitFor(() => {
      expect(
        screen.getByText('Sadece .edu.tr uzantılı üniversite e-postaları kabul edilmektedir.'),
      ).toBeInTheDocument()
    })
  })

  it('does not show .edu.tr warning for valid university email', async () => {
    const user = userEvent.setup({ delay: null })
    renderWithProviders(<RegisterPage />)

    const emailInput = screen.getByPlaceholderText('ornek@uni.edu.tr')
    await user.type(emailInput, 'student@uni.edu.tr')

    await waitFor(() => {
      expect(
        screen.queryByText('Sadece .edu.tr uzantılı üniversite e-postaları kabul edilmektedir.'),
      ).not.toBeInTheDocument()
    })
  })

  it('updates password checklist as user types', async () => {
    const user = userEvent.setup({ delay: null })
    renderWithProviders(<RegisterPage />)

    const passwordInput = screen.getByPlaceholderText(/En az 8 karakter/)
    await user.type(passwordInput, 'Aa1')

    expect(screen.getByText('En az 8 karakter')).toBeInTheDocument()
    expect(screen.getByText('En az bir büyük harf (A-Z)')).toBeInTheDocument()
    expect(screen.getByText('En az bir küçük harf (a-z)')).toBeInTheDocument()
    expect(screen.getByText('En az bir sayı')).toBeInTheDocument()
    expect(screen.getByText(/tüm şifre kurallarını sağlamalısınız/)).toBeInTheDocument()

    await user.type(passwordInput, 'aaaaa')

    await waitFor(() => {
      expect(screen.queryByText(/tüm şifre kurallarını sağlamalısınız/)).not.toBeInTheDocument()
    })
  })

  it('disables submit until password is strong and email is valid', async () => {
    const user = userEvent.setup({ delay: null })
    renderWithProviders(<RegisterPage />)

    const submit = screen.getByRole('button', { name: 'Kayıt ol' })
    expect(submit).toBeDisabled()

    await user.type(screen.getByPlaceholderText('kullanici_adi'), 'testuser')
    await user.type(screen.getByPlaceholderText('ornek@uni.edu.tr'), 'user@gmail.com')
    await user.type(screen.getByPlaceholderText(/En az 8 karakter/), 'Password1')

    await waitFor(() => {
      expect(submit).toBeDisabled()
    })

    const emailInput = screen.getByPlaceholderText('ornek@uni.edu.tr')
    await user.clear(emailInput)
    await user.type(emailInput, 'student@uni.edu.tr')

    await waitFor(() => {
      expect(submit).not.toBeDisabled()
    })
  })
})
