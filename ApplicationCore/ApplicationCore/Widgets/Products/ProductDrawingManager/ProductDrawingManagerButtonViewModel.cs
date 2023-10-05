using ApplicationCore.Features.Orders.ProductDrawings;
using ApplicationCore.Infrastructure.Bus;

namespace ApplicationCore.Widgets.Products.ProductDrawingManager;

internal class ProductDrawingManagerButtonViewModel {

    private readonly IBus _bus;

    public ProductDrawingManagerButtonViewModel(IBus bus) {
        _bus = bus;
    }

    public async Task<int> GetProductDrawingsCount(Guid productId) {

        var response = await _bus.Send(new GetProductDrawingsCount.Query(productId));

        return response.Match(
            count => count,
            error => 0);

    }

}
