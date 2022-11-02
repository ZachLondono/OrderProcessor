namespace ApplicationCore.Features.Orders.Complete;

public class InvoiceEmailConfiguration {

    public string SenderName { get; set; } = string.Empty; 

    public string SenderEmail { get; set; } = string.Empty;

    public string SenderPassword { get; set; } = string.Empty;

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; }

}