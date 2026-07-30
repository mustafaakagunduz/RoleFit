using RoleFit.Api.Domain;

namespace RoleFit.Api.Services;

public interface IFitAnalyzer
{
    /// <summary>Analyzes how well a CV fits a job description and returns a structured result.</summary>
    Task<FitResult> AnalyzeAsync(string cvText, string jobDescription, CancellationToken cancellationToken = default);
}
