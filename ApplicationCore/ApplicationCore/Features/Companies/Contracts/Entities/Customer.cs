using ApplicationCore.Features.Companies.Contracts.ValueObjects;

namespace ApplicationCore.Features.Companies.Contracts.Entities;

public class Customer {

    public Guid Id { get; }
    public string Name { get; set; }
    public string? OrderNumberPrefix { get; set; }
    public string ShippingMethod { get; set; }
    public Contact ShippingContact { get; set; }
    public Address ShippingAddress { get; set; }
    public Contact BillingContact { get; set; }
    public Address BillingAddress { get; set; }
    public ClosetProSettings ClosetProSettings { get; set; }

    internal Customer(Guid id, string name, string shippingMethod, Contact shippingContact, Address shippingAddress, Contact billingContact, Address billingAddress, string? orderNumberPrefix, ClosetProSettings closetProSettings) {
        Id = id;
        Name = name;
        ShippingMethod = shippingMethod;
        ShippingContact = shippingContact;
        ShippingAddress = shippingAddress;
        BillingContact = billingContact;
        BillingAddress = billingAddress;
        OrderNumberPrefix = orderNumberPrefix;
        ClosetProSettings = closetProSettings;
    }

    public static Customer Create(string name, string shippingMethod, Contact shippingContact, Address shippingAddress, Contact billingContact, Address billingAddress)
        => new(Guid.NewGuid(), name, shippingMethod, shippingContact, shippingAddress, billingContact, billingAddress, null, new());

}
