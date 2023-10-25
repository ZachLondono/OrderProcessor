using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.DovetailDBPackingList;

public class DovetailDrawerBox {
    public int Line { get; set; }
    public int Qty { get; set; }
    public Dimension Height { get; set; }
    public Dimension Width { get; set; }
    public Dimension Depth { get; set; }
}
