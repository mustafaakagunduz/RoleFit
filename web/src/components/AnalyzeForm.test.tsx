import { fireEvent, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { AnalyzeForm } from './AnalyzeForm'

describe('AnalyzeForm', () => {
  it('renders CV and job description fields with a disabled submit button when empty', () => {
    render(<AnalyzeForm onSubmitText={vi.fn()} onSubmitPdf={vi.fn()} />)

    expect(screen.getByLabelText('CV')).toBeInTheDocument()
    expect(screen.getByLabelText('İlan')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Analiz et' })).toBeDisabled()
  })

  it('enables submit and calls onSubmitText once both fields are filled', async () => {
    const user = userEvent.setup()
    const onSubmitText = vi.fn()
    render(<AnalyzeForm onSubmitText={onSubmitText} onSubmitPdf={vi.fn()} />)

    await user.type(screen.getByLabelText('CV'), 'Örnek CV metni')
    await user.type(screen.getByLabelText('İlan'), 'Örnek ilan metni')

    const button = screen.getByRole('button', { name: 'Analiz et' })
    expect(button).toBeEnabled()

    await user.click(button)

    expect(onSubmitText).toHaveBeenCalledWith('Örnek CV metni', 'Örnek ilan metni')
  })

  it('switches to PDF mode and calls onSubmitPdf with the selected file', async () => {
    const user = userEvent.setup()
    const onSubmitPdf = vi.fn()
    render(<AnalyzeForm onSubmitText={vi.fn()} onSubmitPdf={onSubmitPdf} />)

    await user.click(screen.getByRole('button', { name: 'PDF dosyası' }))

    const file = new File(['%PDF-1.4 fake'], 'cv.pdf', { type: 'application/pdf' })
    const fileInput = document.getElementById('cvFile') as HTMLInputElement
    await user.upload(fileInput, file)

    await user.type(screen.getByLabelText('İlan'), 'Örnek ilan metni')

    const button = screen.getByRole('button', { name: 'Analiz et' })
    expect(button).toBeEnabled()

    await user.click(button)

    expect(onSubmitPdf).toHaveBeenCalledWith(file, 'Örnek ilan metni')
  })

  it('rejects a non-PDF file with an inline error', async () => {
    const user = userEvent.setup()
    render(<AnalyzeForm onSubmitText={vi.fn()} onSubmitPdf={vi.fn()} />)

    await user.click(screen.getByRole('button', { name: 'PDF dosyası' }))

    const file = new File(['hello'], 'cv.txt', { type: 'text/plain' })
    const fileInput = document.getElementById('cvFile') as HTMLInputElement
    fireEvent.change(fileInput, { target: { files: [file] } })

    expect(screen.getByRole('alert')).toHaveTextContent('Sadece PDF dosyaları kabul edilir.')
    expect(screen.getByRole('button', { name: 'Analiz et' })).toBeDisabled()
  })
})
