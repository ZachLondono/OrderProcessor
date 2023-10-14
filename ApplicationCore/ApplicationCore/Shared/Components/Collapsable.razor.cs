using Microsoft.AspNetCore.Components;

namespace ApplicationCore.Shared.Components;

public partial class Collapsable {

    [Parameter]
    public RenderFragment? Summary { get; set; }

    [Parameter]
    public RenderFragment? Content { get; set; }

    [Parameter]
    public bool IsHidden { get; set; } = true;

    [Parameter]
    public EventCallback<bool> IsHiddenChanged { get; set; }

    private async Task Toggle() {
        IsHidden = !IsHidden;
        await IsHiddenChanged.InvokeAsync(IsHidden);
        StateHasChanged();
    } 
    
}
