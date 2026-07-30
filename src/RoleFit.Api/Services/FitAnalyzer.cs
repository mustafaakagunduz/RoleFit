using System.Text.Json;
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

    public FitAnalyzer(ILlmClient llmClient)
    {
        _llmClient = llmClient;
    }

    public async Task<FitResult> AnalyzeAsync(string cvText, string jobDescription, CancellationToken cancellationToken = default)
    {
        var systemPrompt = FitResultPrompts.BuildSystemPrompt();
        var userPrompt = FitResultPrompts.BuildUserPrompt(cvText, jobDescription);

        var rawJson = await _llmClient.GetStructuredCompletionAsync(
            systemPrompt,
            userPrompt,
            FitResultPrompts.SchemaName,
            FitResultPrompts.JsonSchema,
            cancellationToken);

        try
        {
            var result = JsonSerializer.Deserialize<FitResult>(rawJson, JsonOptions);
            if (result is null)
            {
                throw new LlmAnalysisException("LLM yanıtı boş bir sonuca dönüştü.");
            }

            return result;
        }
        catch (JsonException ex)
        {
            throw new LlmAnalysisException("LLM yanıtı beklenen FitResult şemasıyla eşleşmiyor.", ex);
        }
    }
}
