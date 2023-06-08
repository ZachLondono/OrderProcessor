namespace ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.LoadAllmoxyWebOrderData;

public interface IAllmoxyClient : IDisposable {

    public Task<string> GetExportAsync(string orderNumber, int index);

}