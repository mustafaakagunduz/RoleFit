using RoleFit.Api.Domain;

namespace RoleFit.Api.Services;

/// <summary>Stub implementation; Faz 3'te gerçek bir LLM çağrısıyla değiştirilecek.</summary>
public class FitAnalyzer : IFitAnalyzer
{
    public Task<FitResult> AnalyzeAsync(string cvText, string jobDescription, CancellationToken cancellationToken = default)
    {
        var result = new FitResult(
            OverallScore: 72,
            Verdict: "moderate",
            Summary: "Aday, rolün beklediği temel becerilerin çoğunu karşılıyor; bazı alanlarda deneyim eksikliği var.",
            MatchedSkills:
            [
                new SkillMatch("C#", "CV'de 3 yıl C#/.NET deneyimi belirtilmiş."),
                new SkillMatch("REST API", "CV'de ASP.NET Core ile API geliştirme deneyimi var.")
            ],
            Gaps:
            [
                new Gap("Docker", "important", "Küçük bir projeyi containerize ederek Docker deneyimi kazan ve CV'ye ekle."),
                new Gap("Bulut deneyimi (Azure/AWS)", "nice_to_have", "Ücretsiz katmanda basit bir deploy yaparak temel bulut deneyimi edin.")
            ],
            SuggestedBullets:
            [
                "ASP.NET Core Web API ile uçtan uca bir servis geliştirip cloud'a deploy etti."
            ]);

        return Task.FromResult(result);
    }
}
