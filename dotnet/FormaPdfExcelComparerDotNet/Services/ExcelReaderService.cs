using ClosedXML.Excel;
using FormaPdfExcelComparerDotNet.Models;

namespace FormaPdfExcelComparerDotNet.Services;

public sealed class ExcelReaderService
{
    public List<ExcelRowData> ReadExcelRows(
        Stream excelStream,
        string evaluationColumnName,
        string? documentKeyColumnName)
    {
        using var workbook = new XLWorkbook(excelStream);
        var worksheet = workbook.Worksheets.First();

        var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var headerRow = worksheet.Row(1);

        foreach (var cell in headerRow.CellsUsed())
        {
            var header = NormalizeHeader(cell.GetString());
            if (!string.IsNullOrWhiteSpace(header))
            {
                headers[header] = cell.Address.ColumnNumber;
            }
        }

        var evaluationHeader = NormalizeHeader(evaluationColumnName);
        if (!headers.TryGetValue(evaluationHeader, out var evaluationColumn))
        {
            var available = string.Join(", ", headers.Keys.OrderBy(x => x));
            throw new InvalidOperationException($"No se encontro la columna '{evaluationColumnName}'. Columnas disponibles: {available}");
        }

        int? keyColumn = null;
        if (!string.IsNullOrWhiteSpace(documentKeyColumnName))
        {
            var keyHeader = NormalizeHeader(documentKeyColumnName);
            if (!headers.TryGetValue(keyHeader, out var keyCol))
            {
                var available = string.Join(", ", headers.Keys.OrderBy(x => x));
                throw new InvalidOperationException($"No se encontro la columna clave '{documentKeyColumnName}'. Columnas disponibles: {available}");
            }
            keyColumn = keyCol;
        }

        var rows = new List<ExcelRowData>();
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;

        for (var rowNumber = 2; rowNumber <= lastRow; rowNumber++)
        {
            var row = worksheet.Row(rowNumber);
            var value = row.Cell(evaluationColumn).Value.ToString();
            string? documentKey = null;

            if (keyColumn.HasValue)
            {
                documentKey = row.Cell(keyColumn.Value).Value.ToString().Trim();
            }

            rows.Add(new ExcelRowData
            {
                ExcelRow = rowNumber,
                Evaluacion2 = value,
                DocumentKey = documentKey
            });
        }

        return rows;
    }

    private static string NormalizeHeader(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }
}
