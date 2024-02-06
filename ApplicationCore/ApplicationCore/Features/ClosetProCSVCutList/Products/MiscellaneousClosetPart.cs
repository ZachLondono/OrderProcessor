using Domain.Companies.ValueObjects;
using Domain.Orders.Enums;
using Domain.Orders.Products.Closets;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;

namespace ApplicationCore.Features.ClosetProCSVCutList.Products;

public class MiscellaneousClosetPart : IClosetProProduct {

    public required int Qty { get; set; }
    public required string Color { get; init; }
    public required string EdgeBandingColor { get; init; }
    public required string Room { get; init; }
    public required decimal UnitPrice { get; init; }
    public required int PartNumber { get; init; }

    public required Dimension Width { get; init; }
    public required Dimension Length { get; init; }
    public required MiscellaneousType Type { get; init; }

    public IProduct ToProduct(ClosetProSettings settings) {

        string sku = Type switch {
            MiscellaneousType.ToeKick => settings.ToeKickSKU,
            MiscellaneousType.Filler or MiscellaneousType.Cleat => "NL1",
            MiscellaneousType.Backing => "BK34",
            MiscellaneousType.ExtraPanel => "PANEL",
            MiscellaneousType.Top => "TOP",
            _ => throw new InvalidOperationException("Unexpected miscellaneous closet part type")
        };

        var material = new ClosetMaterial(Color, ClosetMaterialCore.ParticleBoard);

        return new ClosetPart(Guid.NewGuid(),
                              Qty,
                              UnitPrice,
                              PartNumber,
                              Room,
                              sku,
                              Width,
                              Length,
                              material,
                              null,
                              EdgeBandingColor,
                              "",
                              new Dictionary<string, string>());

    }

}