using ApplicationCore.Features.Companies.Contracts.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace ApplicationCore.Features.Companies.Vendors.Edit;

internal class ExistingVendorModel {

    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;
    public Address Address { get; set; } = new();
    public string Phone { get; set; } = string.Empty;
    public ExportProfile ExportProfile { get; set; } = new();
    public ReleaseProfile ReleaseProfile { get; set; } = new();

}
