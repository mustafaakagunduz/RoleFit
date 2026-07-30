using Microsoft.AspNetCore.Mvc;
using RoleFit.Api.Contracts;
using RoleFit.Api.Domain;
using RoleFit.Api.Services;

namespace RoleFit.Api.Controllers;

[ApiController]
[Route("api/analyze")]
public class AnalyzeController : ControllerBase
{
    private readonly IFitAnalyzer _fitAnalyzer;

    public AnalyzeController(IFitAnalyzer fitAnalyzer)
    {
        _fitAnalyzer = fitAnalyzer;
    }

    [HttpPost]
    public async Task<ActionResult<FitResult>> Analyze(AnalyzeRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CvText))
        {
            return Problem(detail: "cvText boş olamaz.", statusCode: StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(request.JobDescription))
        {
            return Problem(detail: "jobDescription boş olamaz.", statusCode: StatusCodes.Status400BadRequest);
        }

        try
        {
            var result = await _fitAnalyzer.AnalyzeAsync(request.CvText, request.JobDescription, cancellationToken);
            return Ok(result);
        }
        catch (LlmAnalysisException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    }
}
