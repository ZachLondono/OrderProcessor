using ApplicationCore.Features.Orders.Loader.Dialog;

namespace ApplicationCore.Features.Orders.Loader.Providers;

public interface IOrderProvider {

    public IOrderLoadWidgetViewModel? OrderLoadingViewModel { get; set; }

    public Task<OrderData?> LoadOrderData(string source);

}
