namespace ApplicationCore.Features.Orders.Loader;

public interface IOrderLoadingViewModel {

    public void AddLoadingMessage(MessageSeverity severity, string message);

}