using Domain.Companies.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace Companies.Vendors.Edit;

public class ExistingVendorModel {

    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;
    public Address Address { get; set; } = new();
    public string Phone { get; set; } = string.Empty;
    public string LogoFile { get; set; } = string.Empty;
    public byte[] Logo { get; set; } = Array.Empty<byte>();
    public ExportProfile ExportProfile { get; set; } = new();
    public ReleaseProfile ReleaseProfile { get; set; } = new();

    public string GetBas64EncodedImage() => string.Format("data:image/png;base64,{0}", Convert.ToBase64String(Logo));

}
