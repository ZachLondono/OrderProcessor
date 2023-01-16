namespace ApplicationCore.Features.Orders.Release.Handlers.Invoice.Models;

internal class PackingList {

    public int ItemCount { get; set; }

    public DateTime Date { get; set; }

    public string OrderName { get; set; } = string.Empty;

    public string OrderNumber { get; set; } = string.Empty;

    public string SubTotal { get; set; } = string.Empty;

    public string Discount { get; set; } = string.Empty;

    public string NetAmount { get; set; } = string.Empty;

    public string SalesTax { get; set; } = string.Empty;

    public string Total { get; set; } = string.Empty;

    public Company Customer { get; set; } = new();

    public Company Vendor { get; set; } = new();

    public List<Item> Items { get; set; } = new();

}
