namespace ApplicationCore.Features.Orders.OrderRelease;

public class ReleaseConfiguration {

    public List<string> CNCDataFilePaths { get; set; } = new();
    public bool GenerateCNCRelease { get; set; }
    public bool CopyCNCReportToWorkingDirectory { get; set; }
    public bool GeneratePackingList { get; set; }
    public bool GenerateJobSummary { get; set; }
    public bool IncludeProductTablesInSummary { get; set; }
    public bool IncludeSuppliesInSummary { get; set; }
    public bool IncludeInvoiceInRelease { get; set; }
    public string? ReleaseEmailRecipients { get; set; }
    public bool SendReleaseEmail { get; set; }
    public bool IncludeSummaryInEmailBody { get; set; }
    public string? ReleaseFileName { get; set; }
    public string? ReleaseOutputDirectory { get; set; }

    public bool GenerateInvoice { get; set; }
    public string? InvoiceFileName { get; set; }
    public string? InvoiceOutputDirectory { get; set; }
    public string? InvoiceEmailRecipients { get; set; }
    public bool SendInvoiceEmail { get; set; }

}
