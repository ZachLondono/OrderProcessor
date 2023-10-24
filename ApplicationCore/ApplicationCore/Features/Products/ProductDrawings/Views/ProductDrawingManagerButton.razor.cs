using Microsoft.AspNetCore.Components;
using Blazored.Modal;
using Blazored.Modal.Services;
using ApplicationCore.Features.Orders.ProductDrawings.Queries;
using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Features.Orders.ProductDrawings.ViewModels;

namespace ApplicationCore.Features.Products.ProductDrawings.Views;

public partial class ProductDrawingManagerButton {

    [CascadingParameter]
    public IModalService Modal { get; set; } = default!;

    [Parameter]
    public Guid ProductId { get; set; } = Guid.Empty;

    [Inject]
    public ProductDrawingManagerButtonViewModel? DataContext { get; set; }

    protected override async Task OnInitializedAsync() {

        if (DataContext is null) return;

        DataContext.OnPropertyChanged += StateHasChanged;

        DataContext.SetProductId(ProductId);

        await DataContext.RefreshProductDrawingsCount();

    }

    private async Task OpenProductDrawingManager() {
        if (DataContext is null) return;
        await DataContext.OpenProductDrawingManager(Modal);
    }

}