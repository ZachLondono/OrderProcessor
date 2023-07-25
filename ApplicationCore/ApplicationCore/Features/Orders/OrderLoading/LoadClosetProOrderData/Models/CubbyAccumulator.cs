using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.Models;

public class CubbyAccumulator {

    private readonly List<Part> _verticalPanels = new();
    private readonly List<Part> _horizontalPanels = new();
    private Part? _topShelf = null;
    private Part? _bottomShelf = null;

    public void AddVerticalPanel(Part part) {
        _verticalPanels.Add(part);
    }

    public void AddHorizontalPanel(Part part) {
        _horizontalPanels.Add(part);
    }

    public void AddTopShelf(Part part) {
        _topShelf = part;
    }

    public void AddBottomShelf(Part part) {
        _bottomShelf = part;
    }

    public IEnumerable<IProduct> GetProducts(ClosetProPartMapper mapper) {

        if (!_verticalPanels.Any()) {
            throw new InvalidOperationException("Missing vertical cubby dividers");
        }

        if (_topShelf is null) {
            throw new InvalidOperationException("Missing cubby top shelf");
        }

        if (_bottomShelf is null) {
            throw new InvalidOperationException("Missing cubby bottom shelf");
        }

        if (_topShelf.Width != _bottomShelf.Width || _topShelf.Depth != _bottomShelf.Depth) {
            throw new InvalidOperationException("Top and bottom cubby shelves do not match");
        }

        // Top shelf and bottom shelf are divider shelves
        // Vertical panels are vertical divider panels
        // All other shelves are standard fixed shelves

        int dividerCount = _verticalPanels.Count;

        var products = new List<IProduct>() {
            mapper.CreateDividerShelfFromPart(_topShelf, dividerCount, false),
            mapper.CreateDividerShelfFromPart(_bottomShelf, dividerCount, false)
        };

        if (_verticalPanels.Any()) {
            Dimension shelfWidth = Dimension.FromInches((_topShelf.Width - (Dimension.FromInches(0.75) * _verticalPanels.Count).AsInches()) / (_verticalPanels.Count + 1));
            foreach (var shelf in _horizontalPanels) {
                shelf.Quantity = _verticalPanels.Count + 1;
                shelf.Width = shelfWidth.AsInches();
                products.Add(mapper.CreateFixedShelfFromPart(shelf));
            }
        }

        foreach (var divider in _verticalPanels) {
            products.Add(mapper.CreateDividerPanelFromPart(divider));
        }

        return products;

    }

}
