import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import Navbar from './Navbar'
import { useAuthStore } from '../../store/authStore'

vi.mock('./NavbarSearch', () => ({
  default: () => <div data-testid="navbar-search" />,
}))

vi.mock('../../services/api', () => ({
  authApi: {
    logout: vi.fn().mockResolvedValue({}),
  },
}))

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

function renderNavbar() {
  return render(
    <MemoryRouter>
      <Navbar />
    </MemoryRouter>,
  )
}

describe('Navbar mobile menu', () => {
  beforeEach(() => {
    resetAuth()
  })

  it('opens and closes the hamburger menu', async () => {
    const user = userEvent.setup()
    renderNavbar()

    expect(screen.queryByRole('dialog', { name: 'Mobil navigasyon' })).not.toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: 'Menüyü aç' }))
    expect(screen.getByRole('dialog', { name: 'Mobil navigasyon' })).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: 'Menüyü kapat' }))
    expect(screen.queryByRole('dialog', { name: 'Mobil navigasyon' })).not.toBeInTheDocument()
  })

  it('shows guest links when logged out', async () => {
    const user = userEvent.setup()
    renderNavbar()

    await user.click(screen.getByRole('button', { name: 'Menüyü aç' }))
    const menu = screen.getByRole('dialog', { name: 'Mobil navigasyon' })

    expect(within(menu).getByRole('link', { name: /Giriş yap/i })).toHaveAttribute('href', '/login')
    expect(within(menu).getByRole('link', { name: /Kayıt ol/i })).toHaveAttribute('href', '/register')
    expect(within(menu).queryByRole('link', { name: /Profilim/i })).not.toBeInTheDocument()
  })

  it('shows user links when logged in', async () => {
    const user = userEvent.setup()
    resetAuth({
      isLoggedIn: true,
      user: { id: 1, username: 'alice', email: 'a@uni.edu.tr', role: 'User', isEmailVerified: true },
    })

    renderNavbar()

    await user.click(screen.getByRole('button', { name: 'Menüyü aç' }))
    const menu = screen.getByRole('dialog', { name: 'Mobil navigasyon' })

    expect(within(menu).getByRole('link', { name: /Profilim/i })).toHaveAttribute('href', '/profile')
    expect(within(menu).getByRole('button', { name: /Çıkış/i })).toBeInTheDocument()
    expect(within(menu).queryByRole('link', { name: /Giriş yap/i })).not.toBeInTheDocument()
    expect(within(menu).queryByRole('link', { name: /Kayıt ol/i })).not.toBeInTheDocument()
  })
})
