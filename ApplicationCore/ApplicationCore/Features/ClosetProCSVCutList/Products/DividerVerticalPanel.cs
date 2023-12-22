using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Closets;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.ClosetProCSVCutList.Products;

public class DividerVerticalPanel {

    public required int Qty { get; init; }
    public required string Color { get; init; }
    public required string EdgeBandingColor { get; init; }
    public required string Room { get; init; }
    public required decimal UnitPrice { get; init; }
    public required int PartNumber { get; init; }

    public required Dimension Height { get; init; }
    public required Dimension Depth {  get; init; }
    public required VerticalPanelDrilling Drilling { get; init; }

    public IProduct ToProduct() {

        var vertDrillingType = VerticalDividerPanelEndDrillingType.DoubleCams;
        string vertSku = $"PCDV{ClosetProPartMapper.GetDividerPanelSuffix(vertDrillingType)}";

        var material = new ClosetMaterial(Color, ClosetMaterialCore.ParticleBoard);

        return new ClosetPart(Guid.NewGuid(),
                              Qty,
                              UnitPrice,
                              PartNumber,
                              Room,
                              vertSku,
                              Depth,
                              Height,
                              material,
                              null,
                              EdgeBandingColor,
                              "",
                              new Dictionary<string, string>());

    }

}