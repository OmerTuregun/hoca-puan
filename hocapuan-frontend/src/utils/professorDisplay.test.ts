import { describe, expect, it } from 'vitest'
import { displayProfessorName, professorInitials } from './professorDisplay'

describe('displayProfessorName', () => {
  it('prefers firstName and lastName when present', () => {
    expect(
      displayProfessorName({ firstName: 'Ayşe', lastName: 'Yılmaz', fullName: 'Prof. Dr. Ayşe Yılmaz' }),
    ).toBe('Ayşe Yılmaz')
  })

  it('strips duplicate title prefix from fullName', () => {
    expect(
      displayProfessorName({ fullName: 'Prof. Dr. Mehmet Kaya', title: 'Prof. Dr.' }),
    ).toBe('Mehmet Kaya')
  })

  it('returns fullName when no title overlap', () => {
    expect(displayProfessorName({ fullName: 'Mehmet Kaya' })).toBe('Mehmet Kaya')
  })

  it('returns em dash when no name parts exist', () => {
    expect(displayProfessorName({})).toBe('—')
  })
})

describe('professorInitials', () => {
  it('returns first and last initial from display name', () => {
    expect(professorInitials({ firstName: 'Ali', lastName: 'Veli' })).toBe('AV')
  })

  it('returns single initial for one-word names', () => {
    expect(professorInitials({ fullName: 'Madonna' })).toBe('M')
  })
})
