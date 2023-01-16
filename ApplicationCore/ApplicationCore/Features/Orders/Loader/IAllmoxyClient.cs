namespace ApplicationCore.Features.Orders.Loader;

public interface IAllmoxyClient : IDisposable {

    public string GetExport(string orderNumber, int index);

}