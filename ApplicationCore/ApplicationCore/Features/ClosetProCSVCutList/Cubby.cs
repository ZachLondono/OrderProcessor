using ApplicationCore.Features.Companies.Contracts.ValueObjects;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Closets;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.ClosetProCSVCutList;

public class Cubby {

    public required DividerShelf TopDividerShelf { get; init; }
    public required DividerShelf BottomDividerShelf { get; init; }
    public required DividerPanel[] DividerPanels { get; init; }
    public required FixedShelf[] FixedShelves { get; init; }
    public required ClosetMaterial Material { get; init; }
    public required string EdgeBandingColor { get; init; }
    public required string Room { get; init; }

    public record FixedShelf(int Qty, Dimension Width, Dimension Depth, decimal UnitPrice, int ProductNumber) {

        public IProduct ToProduct(ClosetMaterial material, string edgeBandingColor, string room, string sku) {

            return new ClosetPart(Guid.NewGuid(),
                                  Qty,
                                  UnitPrice,
                                  ProductNumber,
                                  room,
                                  sku,
                                  Depth,
                                  Width,
                                  material,
                                  null,
                                  edgeBandingColor,
                                  "",
                                  new Dictionary<string, string>());

        } 

    }

    public record DividerShelf(int Qty, Dimension Width, Dimension Depth, int DividerCount, decimal UnitPrice, int ProductNumber) {

        public IProduct ToProduct(ClosetMaterial material, string edgeBandingColor, string room, string sku) {

            Dictionary<string, string> parameters = new() {
                { "Div1", "0" },
                { "Div2", "0" },
                { "Div3", "0" },
                { "Div4", "0" },
                { "Div5", "0" }
             };

            return new ClosetPart(Guid.NewGuid(),
                                  Qty,
                                  UnitPrice,
                                  ProductNumber,
                                  room,
                                  sku,
                                  Depth,
                                  Width,
                                  material,
                                  null,
                                  edgeBandingColor,
                                  "",
                                  parameters);

        }

    }

    public record DividerPanel(int Qty, Dimension Height, Dimension Depth, decimal UnitPrice, int ProductNumber) {

        public IProduct ToProduct(ClosetMaterial material, string edgeBandingColor, string room, string sku) {

            return new ClosetPart(Guid.NewGuid(),
                                  Qty,
                                  UnitPrice,
                                  ProductNumber,
                                  room,
                                  sku,
                                  Depth,
                                  Height,
                                  material,
                                  null,
                                  edgeBandingColor,
                                  "",
                                  new Dictionary<string, string>());

        }

    }

    public IEnumerable<IProduct> GetProducts(ClosetProSettings settings) {

        var horzDrillingType = HorizontalDividerPanelEndDrillingType.DoubleCams;
        var vertDrillingType = VerticalDividerPanelEndDrillingType.DoubleCams;

        string topSku = $"SF-D{DividerPanels.Length}T{ClosetProPartMapper.GetDividerShelfSuffix(horzDrillingType)}";
        string bottomSku = $"SF-D{DividerPanels.Length}B{ClosetProPartMapper.GetDividerShelfSuffix(horzDrillingType)}";

        var products = new List<IProduct>() {
            TopDividerShelf.ToProduct(Material, EdgeBandingColor, Room, topSku),
            BottomDividerShelf.ToProduct(Material, EdgeBandingColor, Room, bottomSku)
        };

        string vertSku = $"PCDV{ClosetProPartMapper.GetDividerPanelSuffix(vertDrillingType)}";

        products.AddRange(DividerPanels.Select(p => p.ToProduct(Material, EdgeBandingColor, Room, vertSku)));
        products.AddRange(FixedShelves.Select(p => p.ToProduct(Material, EdgeBandingColor, Room, settings.FixedShelfSKU)));

        return products;

    }

}
