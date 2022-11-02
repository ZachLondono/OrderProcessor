namespace ApplicationCore.Features.Orders.Release.Handlers.PackingList.Models;

internal class PackingList {

    public int ItemCount { get; set; }

    public DateTime Date { get; set; }

    public string OrderName { get; set; } = string.Empty;

    public string OrderNumber { get; set; } = string.Empty;

    public string Volume { get; set; } = string.Empty;

    public string Weight { get; set; } = string.Empty;

    public Company Customer { get; set; } = new();

    public Company Vendor { get; set; } = new();

    public List<Item> Items { get; set; } = new();

}
