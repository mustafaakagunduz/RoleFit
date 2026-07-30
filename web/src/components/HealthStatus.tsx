import { useEffect, useState } from 'react'
import { fetchHealth, type HealthResponse } from '../api/health'

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
    return <p>Backend durumu kontrol ediliyor...</p>
  }

  if (state.kind === 'error') {
    return <p role="alert">Backend'e ulaşılamadı: {state.message}</p>
  }

  return (
    <p>
      Backend durumu: {state.data.status} (v{state.data.version}, {state.data.utc})
    </p>
  )
}
