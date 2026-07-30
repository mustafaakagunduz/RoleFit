using Microsoft.AspNetCore.Mvc;
using RoleFit.Api.Contracts;
using RoleFit.Api.Domain;
using RoleFit.Api.Services;

namespace RoleFit.Api.Controllers;

[ApiController]
[Route("api/analyze")]
public class AnalyzeController : ControllerBase
{
    private const long MaxPdfSizeBytes = 10 * 1024 * 1024;

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

        return await RunAnalysisAsync(request.CvText, request.JobDescription, cancellationToken);
    }

    [HttpPost("pdf")]
    [RequestSizeLimit(MaxPdfSizeBytes)]
    public async Task<ActionResult<FitResult>> AnalyzePdf([FromForm] IFormFile? cvFile, [FromForm] string? jobDescription, CancellationToken cancellationToken)
    {
        if (cvFile is null || cvFile.Length == 0)
        {
            return Problem(detail: "cvFile boş olamaz.", statusCode: StatusCodes.Status400BadRequest);
        }

        if (cvFile.Length > MaxPdfSizeBytes)
        {
            return Problem(detail: "PDF dosyası çok büyük (maksimum 10 MB).", statusCode: StatusCodes.Status400BadRequest);
        }

        var isPdf = string.Equals(cvFile.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase)
            || cvFile.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
        if (!isPdf)
        {
            return Problem(detail: "Sadece PDF dosyaları kabul edilir.", statusCode: StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(jobDescription))
        {
            return Problem(detail: "jobDescription boş olamaz.", statusCode: StatusCodes.Status400BadRequest);
        }

        string cvText;
        try
        {
            using var stream = cvFile.OpenReadStream();
            cvText = PdfTextExtractor.ExtractText(stream);
        }
        catch (PdfExtractionException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(cvText))
        {
            return Problem(
                detail: "PDF'ten metin çıkarılamadı; dosya taranmış bir görüntü olabilir.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        return await RunAnalysisAsync(cvText, jobDescription, cancellationToken);
    }

    private async Task<ActionResult<FitResult>> RunAnalysisAsync(string cvText, string jobDescription, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _fitAnalyzer.AnalyzeAsync(cvText, jobDescription, cancellationToken);
            return Ok(result);
        }
        catch (LlmAnalysisException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    }
}
