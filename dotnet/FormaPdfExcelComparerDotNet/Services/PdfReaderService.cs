using System.Text.RegularExpressions;
using FormaPdfExcelComparerDotNet.Models;
using UglyToad.PdfPig;

namespace FormaPdfExcelComparerDotNet.Services;

public sealed class PdfReaderService
{
    public PdfEvaluationData ReadPdfEvaluation(string pdfPath)
    {
        var text = ExtractTextFromPdf(pdfPath);
        return new PdfEvaluationData
        {
            PdfFile = Path.GetFileName(pdfPath),
            PdfPath = pdfPath,
            Evaluado = ExtractEvaluadoFromText(text),
            TextPreview = text.Length > 500 ? text[..500] : text
        };
    }

    private static string ExtractTextFromPdf(string pdfPath)
    {
        using var document = PdfDocument.Open(pdfPath);
        var parts = new List<string>();

        foreach (var page in document.GetPages())
        {
            parts.Add(page.Text);
        }

        return string.Join(Environment.NewLine, parts);
    }

    private static string ExtractEvaluadoFromText(string text)
    {
        var patterns = new[]
        {
            @"\bEvaluado\b\s*[:\-]?\s*([^\r\n]+)",
            @"\bEVALUADO\b\s*[:\-]?\s*([^\r\n]+)"
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                continue;
            }

            var value = match.Groups[1].Value.Trim();
            value = Regex.Split(
                value,
                @"\s{2,}|\b(Fecha|Revision|Revisión|Observacion|Observación|Proyecto|Plano|Código|Codigo|Aprobado|Revisado|Firma|Responsable|Estado)\b",
                RegexOptions.IgnoreCase)[0];

            return CleanValue(value);
        }

        return string.Empty;
    }

    private static string CleanValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return Regex.Replace(value.Trim(), @"\s+", " ");
    }
}
