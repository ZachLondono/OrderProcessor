using Domain.Infrastructure.Bus;
using Microsoft.AspNetCore.Components;

namespace ApplicationCore.Features.Orders.Details.Views;

public partial class OrderHeaderWidget {

    [Parameter]
    public Guid? OrderId { get; set; }

    [Parameter]
    public Action<Error>? OnErrorOccurred { get; set; }

    protected override async Task OnInitializedAsync() {

        DataContext.OnPropertyChanged += StateHasChanged;

        if (OnErrorOccurred is not null) {
            DataContext.OnErrorOccurred += OnErrorOccurred;
        }

        if (OrderId is Guid orderId) {
            await DataContext.LoadOrderHeaderAsync(orderId);
        }

    }

    private async Task OnDueDateChanged(ChangeEventArgs args) {
        string newDueDateStr = args.Value?.ToString() ?? "";

        if (DateTime.TryParse(newDueDateStr, out DateTime newDueDate)) {
            await DataContext.SetDueDateAsync(newDueDate);
        }
    }

}
