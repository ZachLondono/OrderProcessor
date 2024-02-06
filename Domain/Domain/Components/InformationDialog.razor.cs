using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;

namespace Domain.Components;

public partial class InformationDialog {

    [CascadingParameter]
    private BlazoredModalInstance BlazoredModal { get; set; } = default!;

    [Parameter]
    public MessageType Type { get; set; } = MessageType.Information;

    [Parameter]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string Details { get; set; } = string.Empty;

    public enum MessageType {
        Error,
        Warning,
        Information
    }

    private async Task CloseDialog() {
        await BlazoredModal.CloseAsync(ModalResult.Ok());
    }


}
