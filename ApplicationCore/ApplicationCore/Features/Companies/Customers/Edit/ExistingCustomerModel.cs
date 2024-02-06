using Domain.Companies.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace Domain.Companies.Customers.Edit;

internal class ExistingCustomerModel {

    public required Guid Id { get; set; }

    [Required]
    public string? Name { get; set; } = null;
    public string? OrderNumberPrefix { get; set; } = null;
    public string ShippingMethod { get; set; } = string.Empty;
    public Contact ShippingContact { get; set; } = new();
    public Address ShippingAddress { get; set; } = new();
    public Contact BillingContact { get; set; } = new();
    public Address BillingAddress { get; set; } = new();
    public ClosetProSettings ClosetProSettings { get; set; } = new();
    public string? WorkingDirectoryRoot { get; set; } = null;

}
