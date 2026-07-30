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

async function parseFitResultResponse(response: Response): Promise<FitResult> {
  if (!response.ok) {
    const problem = (await response.json().catch(() => null)) as ProblemDetails | null
    throw new Error(problem?.detail ?? `Analiz başarısız oldu (${response.status}).`)
  }

  return response.json() as Promise<FitResult>
}

export async function analyze(request: AnalyzeRequest): Promise<FitResult> {
  const response = await fetch('/api/analyze', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  })

  return parseFitResultResponse(response)
}

export async function analyzePdf(cvFile: File, jobDescription: string): Promise<FitResult> {
  const formData = new FormData()
  formData.append('cvFile', cvFile)
  formData.append('jobDescription', jobDescription)

  const response = await fetch('/api/analyze/pdf', {
    method: 'POST',
    body: formData,
  })

  return parseFitResultResponse(response)
}
