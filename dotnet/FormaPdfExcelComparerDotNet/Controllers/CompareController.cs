using System.IO.Compression;
using FormaPdfExcelComparerDotNet.Services;
using Microsoft.AspNetCore.Mvc;

namespace FormaPdfExcelComparerDotNet.Controllers;

public sealed class CompareController : Controller
{
    private readonly ExcelReaderService _excelReaderService;
    private readonly PdfReaderService _pdfReaderService;
    private readonly ComparisonService _comparisonService;
    private readonly ReportWriterService _reportWriterService;

    public CompareController(
        ExcelReaderService excelReaderService,
        PdfReaderService pdfReaderService,
        ComparisonService comparisonService,
        ReportWriterService reportWriterService)
    {
        _excelReaderService = excelReaderService;
        _pdfReaderService = pdfReaderService;
        _comparisonService = comparisonService;
        _reportWriterService = reportWriterService;
    }

    [HttpPost]
    [RequestSizeLimit(200_000_000)]
    public async Task<IActionResult> Local(
        IFormFile excelFile,
        IFormFile pdfZip,
        string evaluationColumnName = "Evaluacion 2",
        string? documentKeyColumnName = null)
    {
        if (excelFile is null || excelFile.Length == 0)
        {
            return BadRequest("Debe subir un archivo Excel .xlsx.");
        }

        if (pdfZip is null || pdfZip.Length == 0)
        {
            return BadRequest("Debe subir un archivo ZIP con PDFs.");
        }

        if (!excelFile.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("El archivo Excel debe ser .xlsx.");
        }

        if (!pdfZip.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Los PDFs deben subirse dentro de un ZIP.");
        }

        var runId = Guid.NewGuid().ToString("N");
        var workRoot = Path.Combine(Path.GetTempPath(), "FormaPdfExcelComparerDotNet", runId);
        var pdfFolder = Path.Combine(workRoot, "pdfs");
        Directory.CreateDirectory(pdfFolder);

        try
        {
            List<Models.ExcelRowData> excelRows;
            await using (var excelStream = excelFile.OpenReadStream())
            {
                excelRows = _excelReaderService.ReadExcelRows(
                    excelStream,
                    evaluationColumnName,
                    string.IsNullOrWhiteSpace(documentKeyColumnName) ? null : documentKeyColumnName);
            }

            var zipPath = Path.Combine(workRoot, pdfZip.FileName);
            await using (var zipStream = System.IO.File.Create(zipPath))
            {
                await pdfZip.CopyToAsync(zipStream);
            }

            ZipFile.ExtractToDirectory(zipPath, pdfFolder, overwriteFiles: true);

            var pdfPaths = Directory.GetFiles(pdfFolder, "*.pdf", SearchOption.AllDirectories);
            if (pdfPaths.Length == 0)
            {
                return BadRequest("El ZIP no contiene archivos PDF.");
            }

            var pdfData = pdfPaths
                .Select(path => _pdfReaderService.ReadPdfEvaluation(path))
                .ToList();

            var results = _comparisonService.Compare(
                excelRows,
                pdfData,
                !string.IsNullOrWhiteSpace(documentKeyColumnName));

            var reportBytes = _reportWriterService.WriteReport(results);

            return File(
                reportBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "reporte_comparacion.xlsx");
        }
        finally
        {
            if (Directory.Exists(workRoot))
            {
                Directory.Delete(workRoot, recursive: true);
            }
        }
    }
}
