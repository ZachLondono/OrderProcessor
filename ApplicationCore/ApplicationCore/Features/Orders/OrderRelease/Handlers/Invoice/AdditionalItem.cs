namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.Invoice;

internal class AdditionalItem {

    public int Line { get; set; }

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

}
