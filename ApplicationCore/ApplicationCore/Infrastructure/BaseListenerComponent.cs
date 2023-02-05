using Microsoft.AspNetCore.Components;

namespace ApplicationCore.Infrastructure;

public abstract class BaseListenerComponent : ComponentBase, IUIListener, IDisposable {

    [Inject]
    protected IUIBus? UIBus { get; set; }

    protected override void OnInitialized() {
        UIBus?.Register(this);
        base.OnInitialized();
    }

    public void Dispose() {
        UIBus?.UnRegister(this);
        GC.SuppressFinalize(this);
    }
}
