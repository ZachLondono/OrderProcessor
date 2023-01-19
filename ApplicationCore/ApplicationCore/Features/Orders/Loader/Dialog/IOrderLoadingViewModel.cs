namespace ApplicationCore.Features.Orders.Loader.Dialog;

public interface IOrderLoadingViewModel {

    public void AddLoadingMessage(MessageSeverity severity, string message);

}