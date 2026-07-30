using Microsoft.Extensions.Logging.Abstractions;
using RoleFit.Api.Prompts;
using RoleFit.Api.Services;
using Xunit;

namespace RoleFit.Api.Tests.Services;

public class FitAnalyzerTests
{
    private class FakeLlmClient : ILlmClient
    {
        private readonly Dictionary<string, string> _responsesBySchema;
        private readonly Exception? _exceptionToThrow;

        public List<string> RequestedSchemas { get; } = [];

        public FakeLlmClient(Dictionary<string, string> responsesBySchema)
        {
            _responsesBySchema = responsesBySchema;
            _exceptionToThrow = null;
        }

        public FakeLlmClient(Exception exceptionToThrow)
        {
            _responsesBySchema = new Dictionary<string, string>();
            _exceptionToThrow = exceptionToThrow;
        }

        public Task<string> GetStructuredCompletionAsync(
            string systemPrompt,
            string userPrompt,
            string schemaName,
            string jsonSchema,
            CancellationToken cancellationToken = default)
        {
            RequestedSchemas.Add(schemaName);

            if (_exceptionToThrow is not null)
            {
                throw _exceptionToThrow;
            }

            return Task.FromResult(_responsesBySchema[schemaName]);
        }
    }

    private static readonly Dictionary<string, string> ValidResponses = new()
    {
        [MultiStepPrompts.CandidateSkillsSchemaName] = """
            { "skills": [{ "skill": "C#", "evidence": "CV'de belirtilmis." }] }
            """,
        [MultiStepPrompts.RoleRequirementsSchemaName] = """
            { "requirements": [{ "requirement": "Docker", "importance": "important" }] }
            """,
        [FitResultPrompts.SchemaName] = """
            {
              "overallScore": 85,
              "verdict": "strong",
              "summary": "Aday role uygun.",
              "matchedSkills": [{ "skill": "C#", "evidence": "CV'de belirtilmis." }],
              "gaps": [{ "requirement": "Docker", "severity": "important", "suggestion": "Ogren." }],
              "suggestedBullets": ["ASP.NET Core ile API gelistirdi."]
            }
            """,
    };

    private static FitAnalyzer CreateAnalyzer(ILlmClient llmClient) =>
        new(llmClient, NullLogger<FitAnalyzer>.Instance);

    [Fact]
    public async Task AnalyzeAsync_RunsAllThreeStepsAndReturnsParsedFitResult()
    {
        var fakeClient = new FakeLlmClient(ValidResponses);
        var analyzer = CreateAnalyzer(fakeClient);

        var result = await analyzer.AnalyzeAsync("cv", "job");

        Assert.Equal(85, result.OverallScore);
        Assert.Equal("strong", result.Verdict);
        Assert.Single(result.MatchedSkills);
        Assert.Single(result.Gaps);
        Assert.Equal(
            [MultiStepPrompts.CandidateSkillsSchemaName, MultiStepPrompts.RoleRequirementsSchemaName, FitResultPrompts.SchemaName],
            fakeClient.RequestedSchemas);
    }

    [Fact]
    public async Task AnalyzeAsync_WithMalformedStepResponse_ThrowsLlmAnalysisException()
    {
        var responses = new Dictionary<string, string>(ValidResponses)
        {
            [MultiStepPrompts.CandidateSkillsSchemaName] = "not valid json",
        };
        var analyzer = CreateAnalyzer(new FakeLlmClient(responses));

        await Assert.ThrowsAsync<LlmAnalysisException>(() => analyzer.AnalyzeAsync("cv", "job"));
    }

    [Fact]
    public async Task AnalyzeAsync_WhenLlmClientThrows_PropagatesLlmAnalysisException()
    {
        var analyzer = CreateAnalyzer(new FakeLlmClient(new LlmAnalysisException("provider down")));

        await Assert.ThrowsAsync<LlmAnalysisException>(() => analyzer.AnalyzeAsync("cv", "job"));
    }
}
