using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;
using RoleFit.Api.Services;
using Xunit;

namespace RoleFit.Api.Tests.Services;

public class PdfTextExtractorTests
{
    private static byte[] BuildSamplePdf(string text)
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);
        page.AddText(text, 12, new PdfPoint(25, 700), font);
        return builder.Build();
    }

    [Fact]
    public void ExtractText_WithValidPdf_ReturnsPageText()
    {
        var pdfBytes = BuildSamplePdf("RoleFit test CV");
        using var stream = new MemoryStream(pdfBytes);

        var text = PdfTextExtractor.ExtractText(stream);

        Assert.Contains("RoleFit test CV", text);
    }

    [Fact]
    public void ExtractText_WithGarbageBytes_ThrowsPdfExtractionException()
    {
        using var stream = new MemoryStream([1, 2, 3, 4, 5]);

        Assert.Throws<PdfExtractionException>(() => PdfTextExtractor.ExtractText(stream));
    }
}
