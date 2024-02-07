using Domain.Companies.ValueObjects;
using Domain.Orders.Enums;
using Domain.Orders.Entities.Products.Closets;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;

namespace OrderLoading.ClosetProCSVCutList.Products;

public class Shelf : IClosetProProduct {

    public required int Qty { get; set; }
    public required string Color { get; init; }
    public required string EdgeBandingColor { get; init; }
    public required string Room { get; init; }
    public required decimal UnitPrice { get; init; }
    public required int PartNumber { get; init; }

    public required Dimension Width { get; init; }
    public required Dimension Depth { get; init; }
    public required ShelfType Type { get; init; }
    public required bool ExtendBack { get; init; }

    public IProduct ToProduct(ClosetProSettings settings) {

        string sku = Type switch {
            ShelfType.Adjustable => settings.AdjustableShelfSKU,
            ShelfType.Fixed => settings.FixedShelfSKU,
            ShelfType.Shoe => GetShoeShelfSku(),
            _ => throw new InvalidOperationException("Unexpected shelf type")
        };

        ClosetMaterial material = new(Color, ClosetMaterialCore.ParticleBoard);
        ClosetPaint? paint = null;
        string comment = "";
        Dictionary<string, string> parameters = [];

        return new ClosetPart(Guid.NewGuid(),
                              Qty,
                              UnitPrice,
                              PartNumber,
                              Room,
                              sku,
                              Depth,
                              Width,
                              material,
                              paint,
                              EdgeBandingColor,
                              comment,
                              parameters);

    }

    public string GetShoeShelfSku() => Depth.AsInches() switch {
        12 => "SS12-TAG",
        14 => "SS14-TAG",
        16 => "SS16-TAG",
        _ => "SS12-TAG" // TODO: Add custom depth shoe shelves
    };

}
