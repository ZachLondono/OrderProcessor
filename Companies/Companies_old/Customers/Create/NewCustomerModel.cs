using Domain.Companies.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace ApplicationCore.Features.Companies.Customers.Create;

internal class NewCustomerModel {

    [Required]
    public string? Name { get; set; } = null;
    public int? AllmoxyId { get; set; } = null;
    public string ShippingMethod { get; set; } = string.Empty;
    public Contact ShippingContact { get; set; } = new();
    public Address ShippingAddress { get; set; } = new();
    public Contact BillingContact { get; set; } = new();
    public Address BillingAddress { get; set; } = new();

}
