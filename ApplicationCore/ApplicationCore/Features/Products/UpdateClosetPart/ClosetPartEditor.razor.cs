using ApplicationCore.Features.Orders.Shared.Domain.Products.Closets;
using Blazored.Modal;
using Microsoft.AspNetCore.Components;

namespace ApplicationCore.Features.Products.UpdateClosetPart;

public partial class ClosetPartEditor {

    [EditorRequired]
    [Parameter]
    public ClosetPart? Product { get; set; }

    [CascadingParameter]
    private BlazoredModalInstance BlazoredModal { get; set; } = default!;

    [Inject]
    public ClosetPartEditorViewModel? DataContext { get; set; }

    protected override void OnInitialized() {

        if (DataContext is null)
            return;

        DataContext.OnPropertyChanged += StateHasChanged;
        DataContext.CloseAsync += BlazoredModal.CloseAsync;

        // TODO: load a model specifically for the editor
        DataContext.Product = Product;

    }

    public Task Cancel() => BlazoredModal.CloseAsync();

}
