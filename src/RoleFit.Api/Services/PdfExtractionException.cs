namespace RoleFit.Api.Services;

/// <summary>Thrown when a PDF file can't be read or no text could be extracted from it.</summary>
public class PdfExtractionException : Exception
{
    public PdfExtractionException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
