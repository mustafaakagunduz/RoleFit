using UglyToad.PdfPig;

namespace RoleFit.Api.Services;

public static class PdfTextExtractor
{
    /// <summary>Extracts plain text from every page of a PDF stream.</summary>
    public static string ExtractText(Stream pdfStream)
    {
        try
        {
            using var document = PdfDocument.Open(pdfStream);
            var pageTexts = document.GetPages().Select(page => page.Text);
            return string.Join("\n", pageTexts);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new PdfExtractionException("PDF dosyasından metin çıkarılamadı; dosya bozuk veya şifreli olabilir.", ex);
        }
    }
}
