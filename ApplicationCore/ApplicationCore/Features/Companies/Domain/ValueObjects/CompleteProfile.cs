namespace ApplicationCore.Features.Companies.Domain.ValueObjects;

public class CompleteProfile {

    public bool EmailInvoice { get; set; }

    public string InvoicePDFDirectory { get; set; } = string.Empty;

    public string EmailSenderName { get; set; } = string.Empty;

    public string EmailSenderEmail { get; set; } = string.Empty;

    public string EmailSenderPassword { get; set; } = string.Empty;

    // TODO: invoice email template

}