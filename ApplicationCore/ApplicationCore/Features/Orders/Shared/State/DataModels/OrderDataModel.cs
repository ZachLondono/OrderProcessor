using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.Shared.State.DataModels;

public class OrderDataModel {

    public string Source { get; set; } = string.Empty;

    public string Number { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public Status Status { get; set; } = Status.UNKNOWN;

    public string CustomerName { get; set; } = string.Empty;

    public Guid VendorId { get; set; }

    public string ProductionNote { get; set; } = string.Empty;

    public string CustomerComment { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; }

    public DateTime? ReleaseDate { get; set; }

    public DateTime? ProductionDate { get; set; }

    public DateTime? CompleteDate { get; set; }

    public decimal Tax { get; set; }

    public decimal PriceAdjustment { get; set; }

    public bool Rush { get; set; }

    public IDictionary<string, string> Info { get; set; } = new Dictionary<string, string>();

    public string ShippingMethod { get; set; } = string.Empty;

    public decimal ShippingPrice { get; set; }

    public string ShippingContact { get; set; } = string.Empty;

    public string ShippingPhoneNumber { get; set; } = string.Empty;

    public Guid ShippingAddressId { get; set; }

    public string? InvoiceEmail { get; set; } = null;

    public string BillingPhoneNumber { get; set; } = string.Empty;

    public Guid BillingAddressId { get; set; }

    public Order AsDomainModel(Guid orderId, Address shippingAddress, Address billingAddress, IEnumerable<IProduct> products, IEnumerable<AdditionalItem> items) {

        ShippingInfo shippingInfo = new() {
            Contact = ShippingContact,
            Method = ShippingMethod,
            PhoneNumber = ShippingPhoneNumber,
            Price = ShippingPrice,
            Address = new() {
                Line1 = shippingAddress.Line1,
                Line2 = shippingAddress.Line2,
                Line3 = shippingAddress.Line3,
                City = shippingAddress.City,
                State = shippingAddress.State,
                Zip = shippingAddress.Zip,
                Country = shippingAddress.Country,
            }
        };

        Customer customer = new() {
            Name = CustomerName,
        };

        BillingInfo billing = new() {
            InvoiceEmail = InvoiceEmail,
            PhoneNumber = BillingPhoneNumber,
            Address = new() {
                Line1 = billingAddress.Line1,
                Line2 = billingAddress.Line2,
                Line3 = billingAddress.Line3,
                City = billingAddress.City,
                State = billingAddress.State,
                Zip = billingAddress.Zip,
                Country = billingAddress.Country,
            }
        };

        return new Order(orderId, Source, Status, Number, Name, customer, VendorId, ProductionNote, CustomerComment, OrderDate, ReleaseDate, ProductionDate, CompleteDate, shippingInfo, billing, Tax, PriceAdjustment, Rush, Info.AsReadOnly(), products, items);

    }

}
