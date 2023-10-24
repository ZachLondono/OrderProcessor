using ApplicationCore.Features.Orders.ProductDrawings.ViewModels;
using Microsoft.AspNetCore.Components;

namespace ApplicationCore.Features.Products.ProductDrawings.Views;

public partial class ProductDrawingManager {

    [Parameter]
    [EditorRequired]
    public Guid ProductId { get; set; }

    [Inject]
    public ProductDrawingManagerViewModel? DataContext { get; set; }

    protected override void OnInitialized() {

        if (DataContext is null) return;
        DataContext.OnPropertyChanged += StateHasChanged;

    }

    protected override async Task OnParametersSetAsync() {

        if (DataContext is null || ProductId == Guid.Empty) {
            return;
        }

        await DataContext.Loaded(ProductId);

    }

}
