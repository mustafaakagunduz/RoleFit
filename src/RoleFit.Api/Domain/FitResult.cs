namespace RoleFit.Api.Domain;

public record FitResult(
    int OverallScore,
    string Verdict,
    string Summary,
    IReadOnlyList<SkillMatch> MatchedSkills,
    IReadOnlyList<Gap> Gaps,
    IReadOnlyList<string> SuggestedBullets);
