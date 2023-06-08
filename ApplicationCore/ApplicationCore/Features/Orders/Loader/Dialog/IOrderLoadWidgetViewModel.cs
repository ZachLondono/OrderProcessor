namespace ApplicationCore.Features.Orders.Loader.Dialog;

public interface IOrderLoadWidgetViewModel {

    public void AddLoadingMessage(MessageSeverity severity, string message);

}