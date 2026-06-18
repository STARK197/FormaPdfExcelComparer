namespace FormaPdfExcelComparerDotNet.Models;

public sealed class ComparisonResultData
{
    public int ExcelRow { get; set; }
    public object? Evaluacion2Excel { get; set; }
    public string PdfFile { get; set; } = string.Empty;
    public string EvaluadoPdf { get; set; } = string.Empty;
    public string Resultado { get; set; } = string.Empty;
    public string Observacion { get; set; } = string.Empty;
}
