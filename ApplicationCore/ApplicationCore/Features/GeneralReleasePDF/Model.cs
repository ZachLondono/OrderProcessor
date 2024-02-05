namespace ApplicationCore.Features.GeneralReleasePDF;

internal class Model {

    private string _reportFilePath = string.Empty;
    public string ReportFilePath {
        get => _reportFilePath;
        set {
            _reportFilePath = value;
            FileName = Path.GetFileNameWithoutExtension(ReportFilePath);
        }
    }

    public DateTime OrderDate { get; set; } = DateTime.Now;
    public DateTime? DueDate { get; set; } = null;
    public string CustomerName { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string OutputDirectory { get; set; } = string.Empty;
    public bool SendEmail { get; set; } = false;
    public string EmailRecipients { get; set; } = string.Empty;

}
