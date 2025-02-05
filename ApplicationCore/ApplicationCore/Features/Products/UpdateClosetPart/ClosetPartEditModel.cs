using Domain.Orders.Entities.Products.Closets;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;

namespace ApplicationCore.Features.Products.UpdateClosetPart;

public class ClosetPartEditModel {

    public required Guid Id { get; init; }
    public required int Qty { get; set; }
    public required decimal UnitPrice { get; set; }
    public required int ProductNumber { get; set; }
    public required string Room { get; set; }
    public required string SKU { get; set; }
    public required double Width { get; set; }
    public required double Length { get; set; }

    public required string MaterialFinish { get; set; }
    public required ClosetMaterialCore MaterialCore { get; set; }

    public required bool IsPainted { get; set; }
    public required string PaintedColor { get; set; }
    public required PaintedSide PaintedSide { get; set; }

    public required string EdgeBandingColor { get; set; }
    public required string Comment { get; set; }
    public required bool InstallCams { get; set; }

    public List<AskParameter> AskParameters { get; private set; } = [];

    public List<string> ProductionNotes { get; set; } = [];

    public static ClosetPartEditModel FromProduct(ClosetPart part)
        => new() {
            Id = part.Id,
            Qty = part.Qty,
            UnitPrice = part.UnitPrice,
            ProductNumber = part.ProductNumber,
            Room = part.Room,
            SKU = part.SKU,
            Width = part.Width.AsMillimeters(),
            Length = part.Length.AsMillimeters(),
            MaterialFinish = part.Material.Finish,
            MaterialCore = part.Material.Core,
            IsPainted = part.Paint is not null,
            PaintedColor = part.Paint?.Color ?? "",
            PaintedSide = part.Paint?.Side ?? PaintedSide.None,
            EdgeBandingColor = part.EdgeBandingColor,
            Comment = part.Comment,
            InstallCams = part.InstallCams,
            ProductionNotes = part.ProductionNotes,
            AskParameters = part.Parameters.Select(AskParameter.FromKeyValuePair).ToList(),
        };

    public ClosetPart ToProduct()
        => new(Id, Qty, UnitPrice, ProductNumber, Room, SKU, Dimension.FromMillimeters(Width), Dimension.FromMillimeters(Length),
            new(MaterialFinish, MaterialCore), IsPainted ? new(PaintedColor, PaintedSide) : null,
            EdgeBandingColor, Comment, InstallCams, AskParameters.ToDictionary(p => p.Name, p => p.Value));

}
