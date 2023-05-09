using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.Shared.State.DataModels;

public class OrderDataModel {

    public string Source { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public string WorkingDirectory { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Guid VendorId { get; set; }
    public string CustomerComment { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal Tax { get; set; }
    public decimal PriceAdjustment { get; set; }
    public bool Rush { get; set; }
    public IDictionary<string, string> Info { get; set; } = new Dictionary<string, string>();

    public string ShippingMethod { get; set; } = string.Empty;
    public decimal ShippingPrice { get; set; }
    public string ShippingContact { get; set; } = string.Empty;
    public string ShippingPhoneNumber { get; set; } = string.Empty;
    public string ShippingLine1 { get; set; } = string.Empty;
    public string ShippingLine2 { get; set; } = string.Empty;
    public string ShippingLine3 { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingState { get; set; } = string.Empty;
    public string ShippingZip { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = string.Empty;

    public string? InvoiceEmail { get; set; } = null;
    public string BillingPhoneNumber { get; set; } = string.Empty;
    public string BillingLine1 { get; set; } = string.Empty;
    public string BillingLine2 { get; set; } = string.Empty;
    public string BillingLine3 { get; set; } = string.Empty;
    public string BillingCity { get; set; } = string.Empty;
    public string BillingState { get; set; } = string.Empty;
    public string BillingZip { get; set; } = string.Empty;
    public string BillingCountry { get; set; } = string.Empty;

    public Order ToDomainModel(Guid orderId, IEnumerable<IProduct> products, IEnumerable<AdditionalItem> items) {

        ShippingInfo shippingInfo = new() {
            Contact = ShippingContact,
            Method = ShippingMethod,
            PhoneNumber = ShippingPhoneNumber,
            Price = ShippingPrice,
            Address = new() {
                Line1 = ShippingLine1,
                Line2 = ShippingLine2,
                Line3 = ShippingLine3,
                City = ShippingCity,
                State = ShippingState,
                Zip = ShippingZip,
                Country = ShippingCountry,
            }
        };

        BillingInfo billing = new() {
            InvoiceEmail = InvoiceEmail,
            PhoneNumber = BillingPhoneNumber,
            Address = new() {
                Line1 = BillingLine1,
                Line2 = BillingLine2,
                Line3 = BillingLine3,
                City = BillingCity,
                State = BillingState,
                Zip = BillingZip,
                Country = BillingCountry,
            }
        };

        var order = new Order(orderId, Source, Number, Name, Note, WorkingDirectory, CustomerId, VendorId, CustomerComment, OrderDate, shippingInfo, billing, Tax, PriceAdjustment, Rush, Info.AsReadOnly(), products, items);
        return order;

    }

    public static string GetQueryById()
        => @"SELECT

                orders.id,
                orders.number,
                orders.name,
                orders.note,
                orders.working_directory AS WorkingDirectory,
                orders.customer_id AS CustomerId,
                orders.vendor_id AS VendorId,
                orders.customer_comment AS CustomerComment,
                orders.order_date AS OrderDate,
                orders.info,
                orders.tax,
                orders.price_adjustment AS PriceAdjustment,
                orders.rush,
                orders.shipping_method AS ShippingMethod,
                orders.shipping_contact AS ShippingContact,
                orders.shipping_phone_number AS ShippingPhoneNumber,
                orders.shipping_price AS ShippingPrice,
                shipping_address.line1 AS ShippingLine1,
                shipping_address.line2 AS ShippingLine2,
                shipping_address.line3 AS ShippingLine3,
                shipping_address.city AS ShippingCity,
                shipping_address.state AS ShippingState,
                shipping_address.zip AS ShippingZip,
                shipping_address.country AS ShippingCountry,
                orders.invoice_email AS InvoiceEmail,
                orders.billing_phone_number AS BillingPhoneNumber,
                billing_address.line1 AS BillingLine1,
                billing_address.line2 AS BillingLine2,
                billing_address.line3 AS BillingLine3,
                billing_address.city AS BillingCity,
                billing_address.state AS BillingState,
                billing_address.zip AS BillingZip,
                billing_address.country AS BillingCountry

            FROM orders
                LEFT JOIN addresses AS shipping_address ON shipping_address.id = orders.shipping_address_id
                LEFT JOIN addresses AS billing_address ON billing_address.id = orders.billing_address_id
            WHERE
                orders.id = @Id;";
}
