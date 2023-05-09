namespace ApplicationCore.Features.Companies.Contracts.ValueObjects;

public record ReleaseProfile {

    public bool GeneratePackingList { get; set; }
    public bool GenerateJobSummary { get; set; }
    public bool IncludeInvoice { get; set; }
    public bool SendReleaseEmail { get; set; }
    public string ReleaseEmailRecipients { get; set; } = string.Empty;

    public bool GenerateInvoice { get; set; }
    public string InvoiceEmailRecipients { get; set; } = string.Empty;
    public bool SendInvoiceEmail { get; set; }

}
