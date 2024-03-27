namespace ApplicationCore.Features.ClosetOrderRelease;

public class ClosetOrderReleaseOptions {

    public bool AddExistingWSXMLReport { get; set; }
    public string WSXMLReportFilePath { get; set; } = string.Empty;

    public string WorkbookFilePath { get; set; } = string.Empty;
    public bool IncludeCover { get; set; }
    public bool IncludePackingList { get; set; }
    public bool IncludePartList { get; set; }
    public bool IncludeDBList { get; set; }
    public bool IncludeMDFList { get; set; }
    public bool IncludeSummary { get; set; }

    public string FileName { get; set; } = string.Empty;
    public string OutputDirectory { get; set; } = string.Empty;

    public bool SendEmail { get; set; }
    public bool PreviewEmail { get; set; }
    public string EmailRecipients { get; set; } = string.Empty;

    public string InvoiceDirectory { get; set; } = string.Empty;
    public bool InvoicePDF { get; set; }

    public bool SendInvoiceEmail { get; set; }
    public bool PreviewInvoiceEmail { get; set; }
    public string InvoiceEmailRecipients { get; set; } = string.Empty;

    public bool SendAcknowledgementEmail { get; set; }
    public bool PreviewAcknowledgementEmail { get; set; }
    public string AcknowledgmentEmailRecipients { get; set; } = string.Empty;

    public bool SendDovetailReleaseEmail { get; set; }
    public bool PreviewDovetailReleaseEmail { get; set; }
    public string DovetailReleaseEmailRecipients { get; set; } = string.Empty;

}
