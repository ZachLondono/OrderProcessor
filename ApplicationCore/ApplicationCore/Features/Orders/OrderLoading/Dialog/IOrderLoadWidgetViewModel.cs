namespace ApplicationCore.Features.Orders.OrderLoading.Dialog;

public interface IOrderLoadWidgetViewModel {

    public void AddLoadingMessage(MessageSeverity severity, string message);

}