using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.PackingList;

internal class FivePieceDoorItem {

    public int Line { get; set; }

    public int Qty { get; set; }

    public string Description { get; set; } = string.Empty;

    public Dimension Height { get; set; }

    public Dimension Width { get; set; }
}