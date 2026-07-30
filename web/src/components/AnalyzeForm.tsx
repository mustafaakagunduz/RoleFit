import { useState } from 'react'

interface AnalyzeFormProps {
  onSubmit: (cvText: string, jobDescription: string) => void
  disabled?: boolean
}

export function AnalyzeForm({ onSubmit, disabled }: AnalyzeFormProps) {
  const [cvText, setCvText] = useState('')
  const [jobDescription, setJobDescription] = useState('')

  const canSubmit = cvText.trim().length > 0 && jobDescription.trim().length > 0 && !disabled

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    if (!canSubmit) return
    onSubmit(cvText, jobDescription)
  }

  return (
    <form onSubmit={handleSubmit}>
      <label htmlFor="cvText">CV</label>
      <textarea
        id="cvText"
        value={cvText}
        onChange={(e) => setCvText(e.target.value)}
        rows={8}
      />

      <label htmlFor="jobDescription">İlan</label>
      <textarea
        id="jobDescription"
        value={jobDescription}
        onChange={(e) => setJobDescription(e.target.value)}
        rows={8}
      />

      <button type="submit" disabled={!canSubmit}>
        Analiz et
      </button>
    </form>
  )
}
