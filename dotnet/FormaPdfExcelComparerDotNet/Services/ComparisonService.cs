using FormaPdfExcelComparerDotNet.Models;

namespace FormaPdfExcelComparerDotNet.Services;

public sealed class ComparisonService
{
    public List<ComparisonResultData> Compare(
        List<ExcelRowData> excelRows,
        List<PdfEvaluationData> pdfData,
        bool useDocumentKey)
    {
        var results = new List<ComparisonResultData>();
        var pdfsWithEvaluado = pdfData
            .Where(pdf => !string.IsNullOrWhiteSpace(NormalizeValue(pdf.Evaluado)))
            .ToList();

        foreach (var excelRow in excelRows)
        {
            var excelValue = excelRow.Evaluacion2;
            var excelNormalized = NormalizeValue(excelValue);

            if (string.IsNullOrWhiteSpace(excelNormalized))
            {
                results.Add(new ComparisonResultData
                {
                    ExcelRow = excelRow.ExcelRow,
                    Evaluacion2Excel = excelValue,
                    Resultado = "VALOR VACIO EN EXCEL",
                    Observacion = "La columna Evaluacion 2 esta vacia."
                });
                continue;
            }

            var candidatePdfs = pdfsWithEvaluado;
            if (useDocumentKey && !string.IsNullOrWhiteSpace(excelRow.DocumentKey))
            {
                candidatePdfs = pdfsWithEvaluado
                    .Where(pdf => pdf.PdfFile.Contains(excelRow.DocumentKey, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (candidatePdfs.Count == 0)
                {
                    results.Add(new ComparisonResultData
                    {
                        ExcelRow = excelRow.ExcelRow,
                        Evaluacion2Excel = excelValue,
                        Resultado = "PDF NO ENCONTRADO",
                        Observacion = $"No se encontro PDF relacionado con la clave '{excelRow.DocumentKey}'."
                    });
                    continue;
                }
            }

            var matchedPdf = candidatePdfs.FirstOrDefault(pdf => NormalizeValue(pdf.Evaluado) == excelNormalized);

            if (matchedPdf is not null)
            {
                results.Add(new ComparisonResultData
                {
                    ExcelRow = excelRow.ExcelRow,
                    Evaluacion2Excel = excelValue,
                    PdfFile = matchedPdf.PdfFile,
                    EvaluadoPdf = matchedPdf.Evaluado,
                    Resultado = "COINCIDE",
                    Observacion = "El valor de Evaluacion 2 coincide con Evaluado del PDF."
                });
            }
            else
            {
                var firstCandidate = useDocumentKey ? candidatePdfs.FirstOrDefault() : null;
                results.Add(new ComparisonResultData
                {
                    ExcelRow = excelRow.ExcelRow,
                    Evaluacion2Excel = excelValue,
                    PdfFile = firstCandidate?.PdfFile ?? string.Empty,
                    EvaluadoPdf = firstCandidate?.Evaluado ?? string.Empty,
                    Resultado = "NO COINCIDE",
                    Observacion = "No se encontro coincidencia entre Evaluacion 2 y Evaluado."
                });
            }
        }

        if (pdfData.Count > 0 && pdfsWithEvaluado.Count == 0)
        {
            results.Add(new ComparisonResultData
            {
                Resultado = "NO SE ENCONTRO EVALUADO EN PDF",
                Observacion = "Ningun PDF contiene texto extraible con el campo Evaluado."
            });
        }

        return results;
    }

    private static string NormalizeValue(object? value)
    {
        if (value is null)
        {
            return string.Empty;
        }

        var text = value.ToString()?.Trim().Replace(",", ".") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        if (double.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var number))
        {
            return Math.Abs(number % 1) < 0.0000001
                ? ((int)number).ToString(System.Globalization.CultureInfo.InvariantCulture)
                : number.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        return text.ToLowerInvariant();
    }
}
