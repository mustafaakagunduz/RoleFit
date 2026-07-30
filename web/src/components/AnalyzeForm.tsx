import { useState } from 'react'
import './AnalyzeForm.css'

type CvMode = 'text' | 'pdf'

interface AnalyzeFormProps {
  onSubmitText: (cvText: string, jobDescription: string) => void
  onSubmitPdf: (cvFile: File, jobDescription: string) => void
  disabled?: boolean
}

export function AnalyzeForm({ onSubmitText, onSubmitPdf, disabled }: AnalyzeFormProps) {
  const [cvMode, setCvMode] = useState<CvMode>('text')
  const [cvText, setCvText] = useState('')
  const [cvFile, setCvFile] = useState<File | null>(null)
  const [fileError, setFileError] = useState<string | null>(null)
  const [jobDescription, setJobDescription] = useState('')

  const hasCv = cvMode === 'text' ? cvText.trim().length > 0 : cvFile !== null
  const canSubmit = hasCv && jobDescription.trim().length > 0 && !disabled

  function handleFileChange(event: React.ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0] ?? null
    if (file && file.type !== 'application/pdf' && !file.name.toLowerCase().endsWith('.pdf')) {
      setCvFile(null)
      setFileError('Sadece PDF dosyaları kabul edilir.')
      return
    }

    setFileError(null)
    setCvFile(file)
  }

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    if (!canSubmit) return

    if (cvMode === 'pdf' && cvFile) {
      onSubmitPdf(cvFile, jobDescription)
    } else {
      onSubmitText(cvText, jobDescription)
    }
  }

  return (
    <form className="analyze-form" onSubmit={handleSubmit}>
      <div className="analyze-form__fields">
        <div className="analyze-form__field">
          <div className="analyze-form__field-header">
            <label htmlFor={cvMode === 'text' ? 'cvText' : 'cvFile'}>CV</label>
            <div className="cv-mode-toggle" role="group" aria-label="CV giriş yöntemi">
              <button
                type="button"
                className={cvMode === 'text' ? 'active' : ''}
                onClick={() => setCvMode('text')}
              >
                Metin
              </button>
              <button
                type="button"
                className={cvMode === 'pdf' ? 'active' : ''}
                onClick={() => setCvMode('pdf')}
              >
                PDF dosyası
              </button>
            </div>
          </div>

          {cvMode === 'text' ? (
            <textarea
              id="cvText"
              placeholder="CV metnini buraya yapıştır..."
              value={cvText}
              onChange={(e) => setCvText(e.target.value)}
              rows={10}
            />
          ) : (
            <div className="pdf-upload">
              <input id="cvFile" type="file" accept=".pdf,application/pdf" onChange={handleFileChange} />
              {cvFile && <p className="pdf-upload__filename">{cvFile.name}</p>}
              {fileError && (
                <p className="pdf-upload__error" role="alert">
                  {fileError}
                </p>
              )}
            </div>
          )}
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
