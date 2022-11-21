namespace ApplicationCore.Features.Companies.Queries.DataModels;

public class CompanyDataModel {

    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string InvoiceEmail { get; set; } = string.Empty;
    public string ConfirmationEmail { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string Line1 { get; set; } = string.Empty;
    public string Line2 { get; set; } = string.Empty;
    public string Line3 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

}
