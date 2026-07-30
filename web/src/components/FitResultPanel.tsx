import type { FitResult } from '../api/analyze'
import './FitResultPanel.css'

interface FitResultPanelProps {
  result: FitResult
}

const verdictLabels: Record<FitResult['verdict'], string> = {
  strong: 'Güçlü uyum',
  moderate: 'Orta uyum',
  weak: 'Düşük uyum',
}

export function FitResultPanel({ result }: FitResultPanelProps) {
  return (
    <section className="fit-result" aria-label="Analiz sonucu">
      <div className="fit-result__header">
        <div className={`fit-result__score fit-result__score--${result.verdict}`}>
          {result.overallScore}
        </div>
        <div>
          <h2>Uyum skoru: {result.overallScore}/100</h2>
          <span className={`fit-result__verdict fit-result__verdict--${result.verdict}`}>
            {verdictLabels[result.verdict]}
          </span>
        </div>
      </div>

      <p className="fit-result__summary">{result.summary}</p>

      {result.matchedSkills.length > 0 && (
        <div>
          <h3>Eşleşen beceriler</h3>
          <div className="skill-chips">
            {result.matchedSkills.map((match) => (
              <span className="skill-chip" key={match.skill}>
                <span className="skill-chip__name">{match.skill}</span>
                <span className="skill-chip__evidence">{match.evidence}</span>
              </span>
            ))}
          </div>
        </div>
      )}

      {result.gaps.length > 0 && (
        <div>
          <h3>Açıklar</h3>
          <ul className="gap-list">
            {result.gaps.map((gap) => (
              <li className="gap-item" key={gap.requirement}>
                <span className="gap-item__title">
                  {gap.requirement}
                  <span className={`severity-badge severity-badge--${gap.severity}`}>
                    {gap.severity.replace('_', ' ')}
                  </span>
                </span>
                <span className="gap-item__suggestion">{gap.suggestion}</span>
              </li>
            ))}
          </ul>
        </div>
      )}

      {result.suggestedBullets.length > 0 && (
        <div>
          <h3>Önerilen CV maddeleri</h3>
          <ul className="bullet-list">
            {result.suggestedBullets.map((bullet) => (
              <li key={bullet}>{bullet}</li>
            ))}
          </ul>
        </div>
      )}
    </section>
  )
}
