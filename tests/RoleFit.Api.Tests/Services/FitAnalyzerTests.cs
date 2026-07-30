using RoleFit.Api.Services;
using Xunit;

namespace RoleFit.Api.Tests.Services;

public class FitAnalyzerTests
{
    private class FakeLlmClient : ILlmClient
    {
        private readonly string _response;
        private readonly Exception? _exceptionToThrow;

        public FakeLlmClient(string response)
        {
            _response = response;
            _exceptionToThrow = null;
        }

        public FakeLlmClient(Exception exceptionToThrow)
        {
            _response = string.Empty;
            _exceptionToThrow = exceptionToThrow;
        }

        public Task<string> GetStructuredCompletionAsync(
            string systemPrompt,
            string userPrompt,
            string schemaName,
            string jsonSchema,
            CancellationToken cancellationToken = default)
        {
            if (_exceptionToThrow is not null)
            {
                throw _exceptionToThrow;
            }

            return Task.FromResult(_response);
        }
    }

    private const string ValidJson = """
        {
          "overallScore": 85,
          "verdict": "strong",
          "summary": "Aday role uygun.",
          "matchedSkills": [{ "skill": "C#", "evidence": "CV'de belirtilmis." }],
          "gaps": [{ "requirement": "Docker", "severity": "important", "suggestion": "Ogren." }],
          "suggestedBullets": ["ASP.NET Core ile API gelistirdi."]
        }
        """;

    [Fact]
    public async Task AnalyzeAsync_WithValidLlmResponse_ReturnsParsedFitResult()
    {
        var analyzer = new FitAnalyzer(new FakeLlmClient(ValidJson));

        var result = await analyzer.AnalyzeAsync("cv", "job");

        Assert.Equal(85, result.OverallScore);
        Assert.Equal("strong", result.Verdict);
        Assert.Single(result.MatchedSkills);
        Assert.Single(result.Gaps);
    }

    [Fact]
    public async Task AnalyzeAsync_WithMalformedLlmResponse_ThrowsLlmAnalysisException()
    {
        var analyzer = new FitAnalyzer(new FakeLlmClient("not valid json"));

        await Assert.ThrowsAsync<LlmAnalysisException>(() => analyzer.AnalyzeAsync("cv", "job"));
    }

    [Fact]
    public async Task AnalyzeAsync_WhenLlmClientThrows_PropagatesLlmAnalysisException()
    {
        var analyzer = new FitAnalyzer(new FakeLlmClient(new LlmAnalysisException("provider down")));

        await Assert.ThrowsAsync<LlmAnalysisException>(() => analyzer.AnalyzeAsync("cv", "job"));
    }
}
