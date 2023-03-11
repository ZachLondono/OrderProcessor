namespace ApplicationCore.Features.Orders.Details.OrderRelease;

public class ReleaseConfiguration {

    public string? CNCDataFilePath { get; set; }
    public bool GenerateCNCRelease { get; set; }
    public bool GeneratePackingList { get; set; }
    public bool GenerateJobSummary { get; set; }
    public bool IncludeInvoiceInRelease { get; set; }
    public string? ReleaseEmailRecipients { get; set; }
    public bool SendReleaseEmail { get; set; }
    public string? ReleaseOutputDirectory { get; set; }

    public bool GenerateInvoice { get; set; }
    public string? InvoiceOutputDirectory { get; set; }
    public string? InvoiceEmailRecipients { get; set; }
    public bool SendInvoiceEmail { get; set; }

}
