using ApplicationCore.Features.Orders.ProductDrawings.Queries;
using ApplicationCore.Features.Products.ProductDrawings.Views;
using Blazored.Modal;
using Blazored.Modal.Services;
using Domain.Infrastructure.Bus;

namespace ApplicationCore.Features.Orders.ProductDrawings.ViewModels;

public class ProductDrawingManagerButtonViewModel {

    private readonly IBus _bus;
    private Guid ProductId = Guid.Empty;

    public Action? OnPropertyChanged { get; set; }

    private int _drawingCount = 0;
    public int DrawingCount {
        get => _drawingCount;
        set {
            _drawingCount = value;
            OnPropertyChanged?.Invoke();
        }
    }

    public ProductDrawingManagerButtonViewModel(IBus bus) {
        _bus = bus;
    }

    public void SetProductId(Guid productId) {

        ProductId = productId;

    }

    public async Task RefreshProductDrawingsCount() {

        var response = await _bus.Send(new GetProductDrawingsCount.Query(ProductId));

        DrawingCount = response.Match(
            count => count,
            error => 0);

    }

    public async Task OpenProductDrawingManager(IModalService modalService) {

        var dialog = modalService.Show<ProductDrawingManager>("Product Drawings", new ModalParameters() { { "ProductId", ProductId } }, new ModalOptions() { DisableBackgroundCancel = true });
        await dialog.Result;
        await RefreshProductDrawingsCount();

    }

}
