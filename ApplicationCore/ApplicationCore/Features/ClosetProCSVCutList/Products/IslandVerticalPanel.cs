using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Closets;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.ClosetProCSVCutList.Products;

public class IslandVerticalPanel : IClosetProProduct {

    public required int Qty { get; init; }
    public required string Color { get; init; }
    public required string EdgeBandingColor { get; init; }
    public required string Room { get; init; }
    public required decimal UnitPrice { get; init; }
    public required int PartNumber { get; init; }

    public required Dimension Height { get; init; }
    public required Dimension PanelDepth { get; init; }
    public required Dimension Side1Depth { get; init; }
    public required VerticalPanelDrilling Drilling { get; init; }

    public IProduct ToProduct() {

        var row1Holes = Side1Depth - Dimension.FromMillimeters(37);

        ClosetPaint? paint = null;
        string comment = string.Empty;

        ClosetMaterial material = new(Color, ClosetMaterialCore.ParticleBoard);

        Dictionary<string, string> parameters = new() {
            { "FINLEFT", Drilling == VerticalPanelDrilling.FinishedLeft ? "1" : "0" },
            { "FINRIGHT", Drilling == VerticalPanelDrilling.FinishedRight ? "1" : "0" },
            { "Row1Holes", row1Holes.AsMillimeters().ToString() }, // TODO: what does it look like when there is a island center panel (probably both vert drill ll and vert drill r are set), and is it even possible to have a center island panel in closet pro??
            { "Row3Holes", "0" } // Optional drawer slide holes
        };

        var sku = Drilling == VerticalPanelDrilling.DrilledThrough ? "PIC" : "PIE";

        return new ClosetPart(Guid.NewGuid(),
                              Qty,
                              UnitPrice,
                              PartNumber,
                              Room,
                              sku,
                              PanelDepth,
                              Height,
                              material,
                              paint,
                              EdgeBandingColor,
                              comment,
                              parameters);

    }

}
