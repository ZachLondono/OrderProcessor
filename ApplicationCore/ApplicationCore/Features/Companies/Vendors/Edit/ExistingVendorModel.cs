using ApplicationCore.Features.Companies.Contracts.ValueObjects;
using ApplicationCore.Infrastructure.Data;
using System.ComponentModel.DataAnnotations;

namespace ApplicationCore.Features.Companies.Vendors.Edit;

internal class ExistingVendorModel {

    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;
    public Address Address { get; set; } = new();
    public string Phone { get; set; } = string.Empty;
    public string LogoFile { get; set; } = string.Empty;
    public byte[] Logo { get; set; } = Array.Empty<byte>();
    public ExportProfile ExportProfile { get; set; } = new();
    public ReleaseProfile ReleaseProfile { get; set; } = new();

    public string EmailSenderName { get; set; } = string.Empty;
    public string EmailSenderEmail { get; set; } = string.Empty;
    public string EmailSenderPassword { get; set; } = string.Empty;

    public EmailSender GetEmailSender() => new(EmailSenderName, EmailSenderEmail, UserDataProtection.Protect(EmailSenderPassword));

    public string GetBas64EncodedImage() => string.Format("data:image/png;base64,{0}", Convert.ToBase64String(Logo));

}
