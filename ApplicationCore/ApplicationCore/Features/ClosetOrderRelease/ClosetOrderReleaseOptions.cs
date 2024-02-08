namespace ApplicationCore.Features.ClosetOrderRelease;

public class ClosetOrderReleaseOptions {

    public bool AddExistingWSXMLReport { get; set; }
    public string WSXMLReportFilePath { get; set; } = string.Empty;

    public string WorkbookFilePath { get; set; } = string.Empty;
    public bool IncludeCover { get; set; }
    public bool IncludePackingList { get; set; }

    public string FileName { get; set; } = string.Empty;
    public string OutputDirectory { get; set; } = string.Empty;

    public bool SendEmail { get; set; }
    public bool PreviewEmail { get; set; }
    public string EmailRecipients { get; set; } = string.Empty;

}
