import { useState } from 'react'
import { HealthStatus } from './components/HealthStatus'
import { AnalyzeForm } from './components/AnalyzeForm'
import { FitResultPanel } from './components/FitResultPanel'
import { ThemeToggle } from './components/ThemeToggle'
import { analyze, analyzePdf, type FitResult } from './api/analyze'
import './App.css'

type AnalysisState =
  | { kind: 'idle' }
  | { kind: 'loading' }
  | { kind: 'error'; message: string }
  | { kind: 'success'; data: FitResult }

function App() {
  const [state, setState] = useState<AnalysisState>({ kind: 'idle' })

  async function handleSubmitText(cvText: string, jobDescription: string) {
    setState({ kind: 'loading' })
    try {
      const data = await analyze({ cvText, jobDescription })
      setState({ kind: 'success', data })
    } catch (error) {
      setState({ kind: 'error', message: (error as Error).message })
    }
  }

  async function handleSubmitPdf(cvFile: File, jobDescription: string) {
    setState({ kind: 'loading' })
    try {
      const data = await analyzePdf(cvFile, jobDescription)
      setState({ kind: 'success', data })
    } catch (error) {
      setState({ kind: 'error', message: (error as Error).message })
    }
  }

  return (
    <div className="page">
      <div className="theme-toggle-slot">
        <ThemeToggle />
      </div>
      <header className="page-header">
        <h1>RoleFit</h1>
        <p className="tagline">
          CV'ni ve bir iş ilanını yapıştır; yapay zekâ uyum skorunu, eşleşen becerileri ve
          kapatman gereken açıkları senin için çıkarsın.
        </p>
        <div className="status-banner">
          <HealthStatus />
        </div>
      </header>

      <AnalyzeForm
        onSubmitText={handleSubmitText}
        onSubmitPdf={handleSubmitPdf}
        disabled={state.kind === 'loading'}
      />

      {state.kind === 'loading' && <p className="loading-hint">Analiz ediliyor…</p>}
      {state.kind === 'error' && (
        <p className="error-banner" role="alert">
          {state.message}
        </p>
      )}
      {state.kind === 'success' && <FitResultPanel result={state.data} />}
    </div>
  )
}

export default App
