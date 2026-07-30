using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RoleFit.Api.Domain;
using RoleFit.Api.Prompts;

namespace RoleFit.Api.Services;

public class FitAnalyzer : IFitAnalyzer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly ILlmClient _llmClient;
    private readonly ILogger<FitAnalyzer> _logger;

    public FitAnalyzer(ILlmClient llmClient, ILogger<FitAnalyzer> logger)
    {
        _llmClient = llmClient;
        _logger = logger;
    }

    public async Task<FitResult> AnalyzeAsync(string cvText, string jobDescription, CancellationToken cancellationToken = default)
    {
        var candidateProfile = await ExtractCandidateSkillsAsync(cvText, cancellationToken);
        var roleRequirements = await ExtractRoleRequirementsAsync(jobDescription, cancellationToken);
        return await CompareAsync(cvText, jobDescription, candidateProfile, roleRequirements, cancellationToken);
    }

    private async Task<CandidateProfile> ExtractCandidateSkillsAsync(string cvText, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Adım 1/3: CV'den aday becerileri çıkarılıyor.");

        var rawJson = await _llmClient.GetStructuredCompletionAsync(
            MultiStepPrompts.BuildCandidateSkillsSystemPrompt(),
            MultiStepPrompts.BuildCandidateSkillsUserPrompt(cvText),
            MultiStepPrompts.CandidateSkillsSchemaName,
            MultiStepPrompts.CandidateSkillsJsonSchema,
            cancellationToken);

        var profile = Deserialize<CandidateProfile>(rawJson, "aday becerileri");
        _logger.LogInformation(
            "Adım 1/3 tamamlandı: {SkillCount} beceri çıkarıldı ({ElapsedMs} ms).",
            profile.Skills.Count,
            stopwatch.ElapsedMilliseconds);
        return profile;
    }

    private async Task<RoleRequirements> ExtractRoleRequirementsAsync(string jobDescription, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Adım 2/3: İlandan rol gereksinimleri çıkarılıyor.");

        var rawJson = await _llmClient.GetStructuredCompletionAsync(
            MultiStepPrompts.BuildRoleRequirementsSystemPrompt(),
            MultiStepPrompts.BuildRoleRequirementsUserPrompt(jobDescription),
            MultiStepPrompts.RoleRequirementsSchemaName,
            MultiStepPrompts.RoleRequirementsJsonSchema,
            cancellationToken);

        var requirements = Deserialize<RoleRequirements>(rawJson, "rol gereksinimleri");
        _logger.LogInformation(
            "Adım 2/3 tamamlandı: {RequirementCount} gereksinim çıkarıldı ({ElapsedMs} ms).",
            requirements.Requirements.Count,
            stopwatch.ElapsedMilliseconds);
        return requirements;
    }

    private async Task<FitResult> CompareAsync(
        string cvText,
        string jobDescription,
        CandidateProfile candidateProfile,
        RoleRequirements roleRequirements,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Adım 3/3: Beceriler ve gereksinimler karşılaştırılıp skorlanıyor.");

        var candidateSkillsJson = JsonSerializer.Serialize(candidateProfile);
        var roleRequirementsJson = JsonSerializer.Serialize(roleRequirements);

        var rawJson = await _llmClient.GetStructuredCompletionAsync(
            MultiStepPrompts.BuildComparisonSystemPrompt(),
            MultiStepPrompts.BuildComparisonUserPrompt(cvText, jobDescription, candidateSkillsJson, roleRequirementsJson),
            FitResultPrompts.SchemaName,
            FitResultPrompts.JsonSchema,
            cancellationToken);

        var result = Deserialize<FitResult>(rawJson, "nihai uyum sonucu");
        _logger.LogInformation(
            "Adım 3/3 tamamlandı: skor {OverallScore}, verdict {Verdict} ({ElapsedMs} ms).",
            result.OverallScore,
            result.Verdict,
            stopwatch.ElapsedMilliseconds);
        return result;
    }

    private static T Deserialize<T>(string rawJson, string stepDescription)
    {
        try
        {
            var value = JsonSerializer.Deserialize<T>(rawJson, JsonOptions);
            if (value is null)
            {
                throw new LlmAnalysisException($"LLM yanıtı ({stepDescription}) boş bir sonuca dönüştü.");
            }

            return value;
        }
        catch (JsonException ex)
        {
            throw new LlmAnalysisException($"LLM yanıtı ({stepDescription}) beklenen şemayla eşleşmiyor.", ex);
        }
    }
}
