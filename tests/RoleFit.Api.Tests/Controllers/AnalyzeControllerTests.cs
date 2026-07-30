using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoleFit.Api.Contracts;
using RoleFit.Api.Controllers;
using RoleFit.Api.Domain;
using RoleFit.Api.Services;
using Xunit;

namespace RoleFit.Api.Tests.Controllers;

public class AnalyzeControllerTests
{
    private class StubAnalyzer : IFitAnalyzer
    {
        public Task<FitResult> AnalyzeAsync(string cvText, string jobDescription, CancellationToken cancellationToken = default)
        {
            var result = new FitResult(80, "strong", "summary", [], [], []);
            return Task.FromResult(result);
        }
    }

    private class FailingAnalyzer : IFitAnalyzer
    {
        public Task<FitResult> AnalyzeAsync(string cvText, string jobDescription, CancellationToken cancellationToken = default)
        {
            throw new LlmAnalysisException("provider down");
        }
    }

    [Fact]
    public async Task Analyze_WithValidInput_ReturnsFitResult()
    {
        var controller = new AnalyzeController(new StubAnalyzer());

        var response = await controller.Analyze(new AnalyzeRequest("CV metni", "İlan metni"), CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var fitResult = Assert.IsType<FitResult>(okResult.Value);
        Assert.Equal("strong", fitResult.Verdict);
    }

    [Theory]
    [InlineData("", "İlan metni")]
    [InlineData("CV metni", "")]
    [InlineData("   ", "İlan metni")]
    public async Task Analyze_WithEmptyInput_ReturnsBadRequest(string cvText, string jobDescription)
    {
        var controller = new AnalyzeController(new StubAnalyzer());

        var response = await controller.Analyze(new AnalyzeRequest(cvText, jobDescription), CancellationToken.None);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
    }

    [Fact]
    public async Task Analyze_WhenAnalyzerThrowsLlmAnalysisException_ReturnsBadGateway()
    {
        var controller = new AnalyzeController(new FailingAnalyzer());

        var response = await controller.Analyze(new AnalyzeRequest("CV metni", "İlan metni"), CancellationToken.None);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status502BadGateway, objectResult.StatusCode);
    }
}
