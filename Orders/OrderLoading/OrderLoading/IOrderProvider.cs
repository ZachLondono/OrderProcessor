namespace OrderLoading;

public interface IOrderProvider {

    public delegate void LogProgress(MessageSeverity severity, string message);

    public Task<OrderData?> LoadOrderData(LogProgress logProgress);

}
