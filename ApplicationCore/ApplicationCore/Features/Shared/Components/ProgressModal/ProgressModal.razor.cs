using Blazored.Modal;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ApplicationCore.Features.Shared.Components.ProgressModal;

public partial class ProgressModal {

    [Parameter]
    public IActionRunner? ActionRunner { get; set; } 

    [CascadingParameter]
    private BlazoredModalInstance? BlazoredModal { get; set; }

    private readonly List<ProgressLogMessage> _messages = new();
    private ElementReference? _messageBox;

    protected override void OnInitialized() {
        DataContext.OnPropertyChanged += () => InvokeAsync(StateHasChanged);
    }

    protected override async Task OnInitializedAsync() {

        if (ActionRunner is not null) {
            ActionRunner.PublishProgressMessage += AddMessageToLog;
            await DataContext.RunAction(ActionRunner);
        }

    }

    private void AddMessageToLog(ProgressLogMessage message) {
        InvokeAsync(() => {
            _messages.Add(message);
            StateHasChanged();
        });

        if (_messageBox is not null) {
            _ = JSRuntime.InvokeVoidAsync("scrollToEnd", new object[] { _messageBox });
        }
    }

    private async Task CloseDialog() {

        if (BlazoredModal is null) {
            return;
        }

        await BlazoredModal.CloseAsync();

    }

}
