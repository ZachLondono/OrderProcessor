using Blazored.Modal;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Domain.Components.ProgressModal;

public partial class ProgressModal {

    [Parameter]
    public IActionRunner? ActionRunner { get; set; }

    [Parameter]
    public string InProgressTitle { get; set; } = "Processing...";

    [Parameter]
    public string CompleteTitle { get; set; } = "Process Complete";

    [CascadingParameter]
    private BlazoredModalInstance? BlazoredModal { get; set; }

    [Parameter]
    public bool ShowProgressBar { get; set; } = false;

    [Parameter]
    public int ProgressValue { get; set; } = 0;

    private readonly List<ProgressLogMessage> _messages = new();
    private ElementReference? _messageBox;

    protected override void OnInitialized() {
        DataContext.OnPropertyChanged += () => InvokeAsync(StateHasChanged);
    }

    protected override async Task OnInitializedAsync() {

        if (ActionRunner is not null) {

            ActionRunner.PublishProgressMessage += AddMessageToLog;
            ActionRunner.SetProgressBarValue += val => {
                InvokeAsync(() => {
                    ProgressValue = val;
                    StateHasChanged();
                });
            };
            ActionRunner.ShowProgressBar += () => {
                InvokeAsync(() => {
                    ProgressValue = 0;
                    ShowProgressBar = true;
                    StateHasChanged();
                });
            };
            ActionRunner.HideProgressBar += () => {
                InvokeAsync(() => {
                    ShowProgressBar = false;
                    StateHasChanged();
                });
            };

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
