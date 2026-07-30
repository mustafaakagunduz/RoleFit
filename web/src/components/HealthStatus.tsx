import { useEffect, useState } from 'react'
import { fetchHealth, type HealthResponse } from '../api/health'
import './HealthStatus.css'

type LoadState =
  | { kind: 'loading' }
  | { kind: 'error'; message: string }
  | { kind: 'success'; data: HealthResponse }

export function HealthStatus() {
  const [state, setState] = useState<LoadState>({ kind: 'loading' })

  useEffect(() => {
    fetchHealth()
      .then((data) => setState({ kind: 'success', data }))
      .catch((error: Error) => setState({ kind: 'error', message: error.message }))
  }, [])

  if (state.kind === 'loading') {
    return (
      <span className="health-pill health-pill--loading">
        <span className="health-dot" />
        Backend durumu kontrol ediliyor...
      </span>
    )
  }

  if (state.kind === 'error') {
    return (
      <span className="health-pill health-pill--error" role="alert">
        <span className="health-dot" />
        Backend'e ulaşılamadı: {state.message}
      </span>
    )
  }

  return (
    <span className="health-pill health-pill--success">
      <span className="health-dot" />
      Backend durumu: {state.data.status} (v{state.data.version})
    </span>
  )
}
