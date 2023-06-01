namespace ApplicationCore.Features.Orders.Loader;

public interface IAllmoxyClient : IDisposable {

    public Task<string> GetExportAsync(string orderNumber, int index);

}