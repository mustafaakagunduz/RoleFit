import { render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { HealthStatus } from './HealthStatus'

describe('HealthStatus', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('shows backend status once loaded', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => ({ status: 'healthy', version: '0.1.0', utc: '2026-07-30T00:00:00Z' }),
      }),
    )

    render(<HealthStatus />)

    await waitFor(() => {
      expect(screen.getByText(/Backend durumu: healthy/)).toBeInTheDocument()
    })
  })

  it('shows an error when the backend is unreachable', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new Error('network error')))

    render(<HealthStatus />)

    await waitFor(() => {
      expect(screen.getByRole('alert')).toHaveTextContent('network error')
    })
  })
})
