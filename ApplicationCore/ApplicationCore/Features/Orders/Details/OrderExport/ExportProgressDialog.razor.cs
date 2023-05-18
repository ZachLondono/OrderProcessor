using static ApplicationCore.Features.Orders.Details.OrderExport.ExportProgressViewModel;
using Microsoft.AspNetCore.Components;
using Blazored.Modal;
using Microsoft.JSInterop;

namespace ApplicationCore.Features.Orders.Details.OrderExport;

public partial class ExportProgressDialog {

    [CascadingParameter]
    private BlazoredModalInstance BlazoredModal { get; set; } = default!;

    [Parameter]
    public ExportConfiguration? Configuration { get; set; } = default;

    private List<ProgressLogMessage> _messages = new();
    private ElementReference _messagebox;

    protected override async Task OnInitializedAsync() {

        DataContext.OnPropertyChanged += () => InvokeAsync(StateHasChanged);
        DataContext.OnMessagePublished += AddMessageToLog;

        if (Configuration is not null) {
            await Task.Run(() => DataContext.ExportOrder(Configuration));
        }

    }

    private void AddMessageToLog(ProgressLogMessage message) {
        InvokeAsync(() => {
            _messages.Add(message);
            StateHasChanged();
        });
        JSRuntime.InvokeVoidAsync("scrollToEnd", new object[] { _messagebox });
    }

    private async Task CloseDialog() {
        await BlazoredModal.CloseAsync();
    }

}
