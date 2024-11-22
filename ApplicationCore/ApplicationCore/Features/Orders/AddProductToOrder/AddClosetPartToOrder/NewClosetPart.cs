using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using OneOf;
using OneOf.Types;

namespace ApplicationCore.Features.Orders.AddProductToOrder.AddClosetPartToOrder;

public class NewClosetPart {

    public int Qty { get; set; }
    public decimal UnitPrice { get; set; }
    public int ProductNumber { get; set; }
    public string Room { get; set; } = string.Empty;

    public double Width { get; set; }
    public double Length { get; set; }

    public string Sku { get; set; } = string.Empty;
    public string MaterialFinish { get; set; } = string.Empty;
    public ClosetMaterialCore MaterialCore { get; set; }
    public OneOf<ClosetPaint, None> Paint { get; set; }
    public string EdgeBandingColor { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public bool InstallCams { get; set; }
    public List<Parameter> Parameters { get; set; } = [];

}
