namespace RoleFit.Api.Services;

/// <summary>Thrown when the LLM provider fails or returns output that can't be parsed into a FitResult.</summary>
public class LlmAnalysisException : Exception
{
    public LlmAnalysisException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
