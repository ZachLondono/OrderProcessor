using Domain.Companies.ValueObjects;
using Domain.Orders.Enums;
using Domain.Orders.Entities.Products.Closets;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;

namespace ApplicationCore.Features.ClosetProCSVCutList.Products;

public class CornerShelf : IClosetProProduct {

    public required int Qty { get; set; }
    public required string Color { get; init; }
    public required string EdgeBandingColor { get; init; }
    public required string Room { get; init; }
    public required decimal UnitPrice { get; init; }
    public required int PartNumber { get; init; }

    public required Dimension ProductWidth { get; init; }
    public required Dimension ProductLength { get; init; }
    public required Dimension RightWidth { get; init; }
    public required Dimension NotchSideLength { get; init; }
    public required CornerShelfType Type { get; init; }

    public IProduct ToProduct(ClosetProSettings settings) {

        var parameters = new Dictionary<string, string> {
            { "RightWidth", RightWidth.AsMillimeters().ToString() },
            { "NotchSideLength", NotchSideLength.AsMillimeters().ToString() },
            { "NotchLeft", "Y" },
        };

        if (Type == CornerShelfType.LAdjustable || Type == CornerShelfType.LFixed) {
            parameters.Add("ShelfRadius", settings.LShelfRadius.AsMillimeters().ToString());
        }

        ClosetPaint? paint = null;
        ClosetMaterial material = new(Color, ClosetMaterialCore.ParticleBoard);
        string comment = "";

        string sku = Type switch {
            CornerShelfType.LAdjustable => settings.LAdjustableShelfSKU,
            CornerShelfType.LFixed => settings.LFixedShelfSKU,
            CornerShelfType.DiagonalAdjustable => settings.DiagonalAdjustableShelfSKU,
            CornerShelfType.DiagonalFixed => settings.DiagonalFixedShelfSKU,
            _ => throw new InvalidOperationException("Unexpected corner shelf type")
        };

        return new ClosetPart(Guid.NewGuid(),
                              Qty,
                              UnitPrice,
                              PartNumber,
                              Room,
                              sku,
                              ProductWidth,
                              ProductLength,
                              material,
                              paint,
                              EdgeBandingColor,
                              comment,
                              parameters);

    }

}
