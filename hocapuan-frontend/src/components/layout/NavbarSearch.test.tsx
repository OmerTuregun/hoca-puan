import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import NavbarSearch from './NavbarSearch'
import { searchApi } from '../../services/api'

const mockNavigate = vi.fn()

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual<typeof import('react-router-dom')>('react-router-dom')
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  }
})

vi.mock('../../services/api', () => ({
  searchApi: {
    suggestions: vi.fn(),
  },
}))

describe('NavbarSearch', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.useFakeTimers({ shouldAdvanceTime: true })
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  it('does not fetch suggestions when query is shorter than 2 characters', async () => {
    const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime })
    render(
      <MemoryRouter>
        <NavbarSearch />
      </MemoryRouter>,
    )

    const input = screen.getByRole('combobox')
    await user.type(input, 'a')

    await vi.advanceTimersByTimeAsync(400)

    expect(searchApi.suggestions).not.toHaveBeenCalled()
    expect(screen.queryByRole('listbox')).not.toBeInTheDocument()
  })

  it('debounces API calls and shows suggestions', async () => {
    const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime })
    vi.mocked(searchApi.suggestions).mockResolvedValue([
      { id: 1, name: 'Prof. Ali', type: 'professor', context: 'Bilgisayar' },
    ])

    render(
      <MemoryRouter>
        <NavbarSearch />
      </MemoryRouter>,
    )

    const input = screen.getByRole('combobox')
    await user.type(input, 'al')

    expect(searchApi.suggestions).not.toHaveBeenCalled()

    await vi.advanceTimersByTimeAsync(350)

    await waitFor(() => {
      expect(searchApi.suggestions).toHaveBeenCalledWith('al')
    })

    await waitFor(() => {
      expect(screen.getByRole('listbox')).toBeInTheDocument()
      expect(screen.getByText('Prof. Ali')).toBeInTheDocument()
    })
  })

  it('waits for debounce before calling API on rapid typing', async () => {
    const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime })
    vi.mocked(searchApi.suggestions).mockResolvedValue([])

    render(
      <MemoryRouter>
        <NavbarSearch />
      </MemoryRouter>,
    )

    const input = screen.getByRole('combobox')
    await user.type(input, 'a')
    await vi.advanceTimersByTimeAsync(100)
    await user.type(input, 'l')
    await vi.advanceTimersByTimeAsync(100)
    await user.type(input, 'i')

    expect(searchApi.suggestions).not.toHaveBeenCalled()

    await vi.advanceTimersByTimeAsync(350)

    await waitFor(() => {
      expect(searchApi.suggestions).toHaveBeenCalledTimes(1)
      expect(searchApi.suggestions).toHaveBeenCalledWith('ali')
    })
  })
})
