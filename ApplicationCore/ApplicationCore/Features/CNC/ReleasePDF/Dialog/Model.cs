namespace ApplicationCore.Features.CNC.ReleasePDF.Dialog;

internal class Model {
    public string ReportFilePath { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public string CustomerName { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string OutputDirectory { get; set; } = string.Empty;
}
