import { useState } from 'react'
import { HealthStatus } from './components/HealthStatus'
import { AnalyzeForm } from './components/AnalyzeForm'
import { FitResultPanel } from './components/FitResultPanel'
import { analyze, type FitResult } from './api/analyze'
import './App.css'

type AnalysisState =
  | { kind: 'idle' }
  | { kind: 'loading' }
  | { kind: 'error'; message: string }
  | { kind: 'success'; data: FitResult }

function App() {
  const [state, setState] = useState<AnalysisState>({ kind: 'idle' })

  async function handleSubmit(cvText: string, jobDescription: string) {
    setState({ kind: 'loading' })
    try {
      const data = await analyze({ cvText, jobDescription })
      setState({ kind: 'success', data })
    } catch (error) {
      setState({ kind: 'error', message: (error as Error).message })
    }
  }

  return (
    <main>
      <h1>RoleFit</h1>
      <HealthStatus />
      <AnalyzeForm onSubmit={handleSubmit} disabled={state.kind === 'loading'} />
      {state.kind === 'loading' && <p>Analiz ediliyor...</p>}
      {state.kind === 'error' && <p role="alert">{state.message}</p>}
      {state.kind === 'success' && <FitResultPanel result={state.data} />}
    </main>
  )
}

export default App
