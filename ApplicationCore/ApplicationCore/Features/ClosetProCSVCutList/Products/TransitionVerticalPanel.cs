using ApplicationCore.Features.Orders.Shared.Domain.Products.Closets;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;

namespace ApplicationCore.Features.ClosetProCSVCutList.Products;

public class TransitionVerticalPanel : IClosetProProduct {

    public required int Qty { get; init; }
    public required string Color { get; init; }
    public required string EdgeBandingColor { get; init; }
    public required string Room { get; init; }
    public required decimal UnitPrice { get; init; }
    public required int PartNumber { get; init; }

    public required Dimension Height { get; init; }
    public required Dimension Depth {  get; init; }
    public required Dimension TransitionDepth { get; init; }
    public required VerticalPanelDrilling Drilling { get; init; }
    public required bool WallHung { get; init; }
    public required bool ExtendBack { get; init; }
    public required bool HasBottomRadius { get; init; }
    public required BaseNotch BaseNotch { get; init; }

    public IProduct ToProduct(Dimension verticalPanelBottomRadius) {

        string sku = "PCDT"; // TODO: add option to do two sided machining

        ClosetPaint? paint = null;
        string comment = string.Empty;

        ClosetMaterial material = new(Color, ClosetMaterialCore.ParticleBoard);

        Dictionary<string, string> parameters = new() {
            { "FINLEFT", Drilling == VerticalPanelDrilling.FinishedLeft ? "1" : "0" },
            { "FINRIGHT", Drilling == VerticalPanelDrilling.FinishedRight ? "1" : "0" },
            { "ExtendBack", ExtendBack ? "19.05" : "0" },
            { "BottomNotchD", BaseNotch.Depth.AsMillimeters().ToString() },
            { "BottomNotchH", BaseNotch.Height.AsMillimeters().ToString() },
            { "WallMount", WallHung ? "1" : "0" },                          // TODO: add option to settings to include wall mounting bracket on wall hung panels
            { "BottomRadius", HasBottomRadius ? verticalPanelBottomRadius.AsMillimeters().ToString() : "0" },
            { "MiddleHoles", (TransitionDepth - Dimension.FromMillimeters(37)).AsMillimeters().ToString() }
        };

        return new ClosetPart(Guid.NewGuid(),
                              Qty,
                              UnitPrice,
                              PartNumber,
                              Room,
                              sku,
                              Depth,
                              Height,
                              material,
                              paint,
                              EdgeBandingColor,
                              comment,
                              parameters);

    }

}
