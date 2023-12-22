using ApplicationCore.Features.ClosetProCSVCutList.Products;
using ApplicationCore.Features.Companies.Contracts.ValueObjects;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

namespace ApplicationCore.Features.ClosetProCSVCutList;

public class Cubby {

    public required DividerShelf TopDividerShelf { get; init; }
    public required DividerShelf BottomDividerShelf { get; init; }
    public required DividerVerticalPanel[] DividerPanels { get; init; }
    public required Shelf[] FixedShelves { get; init; }
    public required ClosetMaterial Material { get; init; }
    public required string EdgeBandingColor { get; init; }
    public required string Room { get; init; }

    public IEnumerable<IProduct> GetProducts(ClosetProSettings settings) {

        var products = new List<IProduct>() {
            TopDividerShelf.ToProduct(),
            BottomDividerShelf.ToProduct()
        };


        products.AddRange(DividerPanels.Select(p => p.ToProduct()));
        products.AddRange(FixedShelves.Select(p => p.ToProduct(settings)));

        return products;

    }

}
