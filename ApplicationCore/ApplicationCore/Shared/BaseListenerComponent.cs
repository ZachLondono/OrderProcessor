using ApplicationCore.Infrastructure;
using Microsoft.AspNetCore.Components;

namespace ApplicationCore.Shared;

public abstract class BaseListenerComponent : ComponentBase, IUIListener, IDisposable {

    [Inject]
    protected IUIBus? UIBus { get; set; }

    protected override void OnInitialized() {
        if (UIBus is not null) UIBus.Register(this);
        base.OnInitialized();
    }

    public void Dispose() {
        if (UIBus is not null)  UIBus.UnRegister(this);
        GC.SuppressFinalize(this);
    }
}
