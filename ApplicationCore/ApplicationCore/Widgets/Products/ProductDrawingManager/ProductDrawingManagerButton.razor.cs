using ApplicationCore.Features.Orders.ProductDrawings;
using ApplicationCore.Infrastructure.Bus;
using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;

namespace ApplicationCore.Widgets.Products.ProductDrawingManager;

public partial class ProductDrawingManagerButton {

    [CascadingParameter]
    public IModalService Modal { get; set; } = default!;

    [Parameter]
    public Guid ProductId { get; set; } = Guid.Empty;

    [Inject]
    public IBus? Bus { get; set; }

    private int _drawingCount = 0;

    protected override async Task OnInitializedAsync() {
        await RefreshDrawingCount();
    }

    private async Task RefreshDrawingCount() {

        if (ProductId == Guid.Empty || Bus is null) return;

        var response = await Bus.Send(new GetProductDrawingsCount.Query(ProductId));

        response.OnSuccess(count => {
            _drawingCount = count;
        });

    }

    public async Task OpenProductDrawingManager() {

        if (ProductId == Guid.Empty) return;

        var dialog = Modal.Show<ProductDrawingManagerWidget>("Product Drawings",
            new ModalParameters() {
                { "ProductId", ProductId }
            },
            new ModalOptions() {
                DisableBackgroundCancel = true
            });

        await dialog.Result;

        await RefreshDrawingCount();
        StateHasChanged();

    }

}
