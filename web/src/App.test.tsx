import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import App from './App'

describe('App', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('shows the analysis result after a successful submit', async () => {
    const user = userEvent.setup()
    vi.stubGlobal(
      'fetch',
      vi.fn().mockImplementation((url: string) => {
        if (url === '/health') {
          return Promise.resolve({
            ok: true,
            json: async () => ({ status: 'healthy', version: '0.1.0', utc: '2026-07-30T00:00:00Z' }),
          })
        }
        return Promise.resolve({
          ok: true,
          json: async () => ({
            overallScore: 85,
            verdict: 'strong',
            summary: 'Aday role çok uygun.',
            matchedSkills: [{ skill: 'C#', evidence: 'CV üzerinde belirtilmiş.' }],
            gaps: [],
            suggestedBullets: [],
          }),
        })
      }),
    )

    render(<App />)

    await user.type(screen.getByLabelText('CV'), 'Örnek CV metni')
    await user.type(screen.getByLabelText('İlan'), 'Örnek ilan metni')
    await user.click(screen.getByRole('button', { name: 'Analiz et' }))

    await waitFor(() => {
      expect(screen.getByText(/Uyum skoru: 85/)).toBeInTheDocument()
    })
  })

  it('shows an error message when the analysis request fails', async () => {
    const user = userEvent.setup()
    vi.stubGlobal(
      'fetch',
      vi.fn().mockImplementation((url: string) => {
        if (url === '/health') {
          return Promise.resolve({
            ok: true,
            json: async () => ({ status: 'healthy', version: '0.1.0', utc: '2026-07-30T00:00:00Z' }),
          })
        }
        return Promise.resolve({
          ok: false,
          status: 502,
          json: async () => ({ detail: 'Sağlayıcı hatası.' }),
        })
      }),
    )

    render(<App />)

    await user.type(screen.getByLabelText('CV'), 'Örnek CV metni')
    await user.type(screen.getByLabelText('İlan'), 'Örnek ilan metni')
    await user.click(screen.getByRole('button', { name: 'Analiz et' }))

    await waitFor(() => {
      expect(screen.getByRole('alert')).toHaveTextContent('Sağlayıcı hatası.')
    })
  })
})
