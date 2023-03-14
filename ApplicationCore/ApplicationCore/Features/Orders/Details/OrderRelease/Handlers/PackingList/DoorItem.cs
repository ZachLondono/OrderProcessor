using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.PackingList;

internal class DoorItem {

    public int Line { get; set; }

    public int Qty { get; set; }

    public string Description { get; set; } = string.Empty;

    public Dimension Height { get; set; }

    public Dimension Width { get; set; }
}

