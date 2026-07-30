export interface SkillMatch {
  skill: string
  evidence: string
}

export interface Gap {
  requirement: string
  severity: 'critical' | 'important' | 'nice_to_have'
  suggestion: string
}

export interface FitResult {
  overallScore: number
  verdict: 'strong' | 'moderate' | 'weak'
  summary: string
  matchedSkills: SkillMatch[]
  gaps: Gap[]
  suggestedBullets: string[]
}

export interface AnalyzeRequest {
  cvText: string
  jobDescription: string
}

export interface ProblemDetails {
  title?: string
  detail?: string
  status?: number
}

export async function analyze(request: AnalyzeRequest): Promise<FitResult> {
  const response = await fetch('/api/analyze', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  })

  if (!response.ok) {
    const problem = (await response.json().catch(() => null)) as ProblemDetails | null
    throw new Error(problem?.detail ?? `Analiz başarısız oldu (${response.status}).`)
  }

  return response.json() as Promise<FitResult>
}
