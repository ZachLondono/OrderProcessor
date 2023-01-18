namespace ApplicationCore.Features.Companies.Domain.ValueObjects;

public class CompleteProfile {

    public required bool EmailInvoice { get; set; }
    public required string InvoicePDFDirectory { get; set; }
    public required string EmailSenderName { get; set; }
    public required string EmailSenderEmail { get; set; }
    public required string EmailSenderPassword { get; set; }

    public static CompleteProfile Default => new() {
        EmailInvoice = false,
        InvoicePDFDirectory = string.Empty,
        EmailSenderEmail = string.Empty,
        EmailSenderPassword = string.Empty,
        EmailSenderName = string.Empty,
    };

}