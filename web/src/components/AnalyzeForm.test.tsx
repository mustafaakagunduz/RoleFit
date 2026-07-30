import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { AnalyzeForm } from './AnalyzeForm'

describe('AnalyzeForm', () => {
  it('renders CV and job description fields with a disabled submit button when empty', () => {
    render(<AnalyzeForm onSubmit={vi.fn()} />)

    expect(screen.getByLabelText('CV')).toBeInTheDocument()
    expect(screen.getByLabelText('İlan')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Analiz et' })).toBeDisabled()
  })

  it('enables submit and calls onSubmit once both fields are filled', async () => {
    const user = userEvent.setup()
    const onSubmit = vi.fn()
    render(<AnalyzeForm onSubmit={onSubmit} />)

    await user.type(screen.getByLabelText('CV'), 'Örnek CV metni')
    await user.type(screen.getByLabelText('İlan'), 'Örnek ilan metni')

    const button = screen.getByRole('button', { name: 'Analiz et' })
    expect(button).toBeEnabled()

    await user.click(button)

    expect(onSubmit).toHaveBeenCalledWith('Örnek CV metni', 'Örnek ilan metni')
  })
})
