using ClosedXML.Excel;
using FormaPdfExcelComparerDotNet.Models;

namespace FormaPdfExcelComparerDotNet.Services;

public sealed class ReportWriterService
{
    public byte[] WriteReport(List<ComparisonResultData> comparisonResults)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Comparacion");

        var headers = new[]
        {
            "Fila Excel",
            "Evaluacion 2 Excel",
            "Archivo PDF",
            "Evaluado PDF",
            "Resultado",
            "Observacion"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        for (var rowIndex = 0; rowIndex < comparisonResults.Count; rowIndex++)
        {
            var result = comparisonResults[rowIndex];
            var excelRowNumber = rowIndex + 2;

            worksheet.Cell(excelRowNumber, 1).Value = result.ExcelRow == 0 ? string.Empty : result.ExcelRow;
            worksheet.Cell(excelRowNumber, 2).Value = result.Evaluacion2Excel?.ToString() ?? string.Empty;
            worksheet.Cell(excelRowNumber, 3).Value = result.PdfFile;
            worksheet.Cell(excelRowNumber, 4).Value = result.EvaluadoPdf;
            worksheet.Cell(excelRowNumber, 5).Value = result.Resultado;
            worksheet.Cell(excelRowNumber, 6).Value = result.Observacion;

            var resultCell = worksheet.Cell(excelRowNumber, 5);
            if (result.Resultado == "COINCIDE")
            {
                resultCell.Style.Fill.BackgroundColor = XLColor.LightGreen;
            }
            else if (result.Resultado == "NO COINCIDE")
            {
                resultCell.Style.Fill.BackgroundColor = XLColor.LightPink;
            }
            else
            {
                resultCell.Style.Fill.BackgroundColor = XLColor.LightYellow;
            }
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
