import { useState } from 'react'
import './AnalyzeForm.css'

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
    <form className="analyze-form" onSubmit={handleSubmit}>
      <div className="analyze-form__fields">
        <div className="analyze-form__field">
          <label htmlFor="cvText">CV</label>
          <textarea
            id="cvText"
            placeholder="CV metnini buraya yapıştır..."
            value={cvText}
            onChange={(e) => setCvText(e.target.value)}
            rows={10}
          />
        </div>

        <div className="analyze-form__field">
          <label htmlFor="jobDescription">İlan</label>
          <textarea
            id="jobDescription"
            placeholder="İş ilanı metnini buraya yapıştır..."
            value={jobDescription}
            onChange={(e) => setJobDescription(e.target.value)}
            rows={10}
          />
        </div>
      </div>

      <button type="submit" className="analyze-form__submit" disabled={!canSubmit}>
        Analiz et
      </button>
    </form>
  )
}
