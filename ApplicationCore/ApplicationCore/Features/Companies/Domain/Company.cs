using ApplicationCore.Features.Companies.Domain.ValueObjects;

namespace ApplicationCore.Features.Companies.Domain;

public class Company {

    public Guid Id { get; }
    public string Name { get; } = string.Empty;
    public Address Address { get; } = new();
    public string PhoneNumber { get; }
    public string InvoiceEmail { get; }
    public string ConfirmationEmail { get; }
    public string ContactName { get; }

    public Company(Guid id, string name, Address address, string phoneNumber, string invoiceEmail, string confirmationEmail, string contactName) {
        Id = id;
        Name = name;
        Address = address;
        PhoneNumber = phoneNumber;
        InvoiceEmail = invoiceEmail;
        ConfirmationEmail = confirmationEmail;
        ContactName = contactName;
    }

    public static Company Create(string name, Address address, string phoneNumber, string invoiceEmail, string confirmationEmail, string contactName) => new(Guid.NewGuid(), name, address, phoneNumber, invoiceEmail, confirmationEmail, contactName);

}
