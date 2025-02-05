using Blazored.Modal;
using Domain.Orders.Entities.Products.Closets;
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

        if (Product is not null) {
            DataContext.EditModel = ClosetPartEditModel.FromProduct(Product);
        }

    }

    public Task Cancel() => BlazoredModal.CloseAsync();

}
