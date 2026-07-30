import type { FitResult } from '../api/analyze'

interface FitResultPanelProps {
  result: FitResult
}

export function FitResultPanel({ result }: FitResultPanelProps) {
  return (
    <section aria-label="Analiz sonucu">
      <h2>
        Uyum skoru: {result.overallScore} ({result.verdict})
      </h2>
      <p>{result.summary}</p>

      <h3>Eşleşen beceriler</h3>
      <ul>
        {result.matchedSkills.map((match) => (
          <li key={match.skill}>
            <strong>{match.skill}</strong>: {match.evidence}
          </li>
        ))}
      </ul>

      <h3>Açıklar</h3>
      <ul>
        {result.gaps.map((gap) => (
          <li key={gap.requirement}>
            <strong>{gap.requirement}</strong> ({gap.severity}): {gap.suggestion}
          </li>
        ))}
      </ul>

      {result.suggestedBullets.length > 0 && (
        <>
          <h3>Önerilen CV maddeleri</h3>
          <ul>
            {result.suggestedBullets.map((bullet) => (
              <li key={bullet}>{bullet}</li>
            ))}
          </ul>
        </>
      )}
    </section>
  )
}
