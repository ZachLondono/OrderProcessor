namespace ApplicationCore.Layouts.MainLayout.DoorOrderRelease;

public class DoorOrderReleaseOptions {

    public bool AddExistingWSXMLReport { get; set; }
    public string WSXMLReportFilePath { get; set; } = string.Empty;

    //public bool AddExistingJSONReport { get; set; }
    //public string JSONReportFilePath { get; set; } = string.Empty;

    public bool GenerateGCodeFromWorkbook { get; set; }

    public string WorkbookFilePath { get; set; } = string.Empty;
    public bool IncludeCover { get; set; }
    public bool IncludePackingList { get; set; }
    public bool IncludeInvoice { get; set; }

    //public bool SendInvoiceEmail { get; set; }

    public string FileName { get; set; } = string.Empty;
    public bool PrintFile { get; set; }
    public string OutputDirectory { get; set; } = string.Empty;

    public bool SendEmail { get; set; } = false;
    public bool PreviewEmail { get; set; } = false;
    public string EmailRecipients { get; set; } = string.Empty;

}
