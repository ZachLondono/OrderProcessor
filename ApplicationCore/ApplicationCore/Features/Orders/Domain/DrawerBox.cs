using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Domain;

public abstract class DrawerBox {

    public int Qty { get; }
    public Dimension Height { get; }
    public Dimension Width { get; }
    public Dimension Depth { get; }
    public DrawerBoxOptions Options { get; }

    public DrawerBox(int qty, Dimension height, Dimension width, Dimension depth, DrawerBoxOptions options) {
        Qty = qty;
        Height = height;
        Width = width;
        Depth = depth;
        Options = options;
    }

}
