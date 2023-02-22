namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.Invoice.Models;

internal class CabinetItem {

    public int Line { get; set; }

    public int Qty { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Height { get; set; } = string.Empty;

    public string Width { get; set; } = string.Empty;

    public string Depth { get; set; } = string.Empty;

    public string Price { get; set; } = string.Empty;

    public string ExtPrice { get; set; } = string.Empty;

}

