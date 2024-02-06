using Domain.Orders.Enums;
using Domain.Orders.Products.Closets;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;

namespace ApplicationCore.Features.ClosetProCSVCutList.Products;

public class DividerShelf : IClosetProProduct {

    public required int Qty { get; set; }
    public required string Color { get; init; }
    public required string EdgeBandingColor { get; init; }
    public required string Room { get; init; }
    public required decimal UnitPrice { get; init; }
    public required int PartNumber { get; init; }

    public required Dimension Width { get; init; }
    public required Dimension Depth { get; init; }
    public required int DividerCount { get; init; }
    public required DividerShelfType Type { get; init; }

    public IProduct ToProduct() {

        Dictionary<string, string> parameters = new() {
                { "Div1", "0" },
                { "Div2", "0" },
                { "Div3", "0" },
                { "Div4", "0" },
                { "Div5", "0" }
             };

        var material = new ClosetMaterial(Color, ClosetMaterialCore.ParticleBoard);

        var horzDrillingType = HorizontalDividerPanelEndDrillingType.DoubleCams;

        string sku = Type switch {
            DividerShelfType.Top => $"SF-D{DividerCount}T{ClosetProPartMapper.GetDividerShelfSuffix(horzDrillingType)}",
            DividerShelfType.Bottom => $"SF-D{DividerCount}B{ClosetProPartMapper.GetDividerShelfSuffix(horzDrillingType)}",
            _ => throw new InvalidOperationException("Unexpected divider shelf type")
        };

        return new ClosetPart(Guid.NewGuid(),
                              Qty,
                              UnitPrice,
                              PartNumber,
                              Room,
                              sku,
                              Depth,
                              Width,
                              material,
                              null,
                              EdgeBandingColor,
                              "",
                              parameters);

    }

}
