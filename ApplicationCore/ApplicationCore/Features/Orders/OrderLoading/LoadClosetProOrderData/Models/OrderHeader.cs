namespace ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.Models;

public record OrderHeader {
    public string Designer { get; init; } = string.Empty; // Contains designer name and designer company
    public string B { get; init; } = string.Empty; // Some information about customer
    public string C { get; init; } = string.Empty; // Might be customer company name or designer company name
    public string CustomerName { get; init; } = string.Empty;
    public string OrderName { get; init; } = string.Empty;
}
