using ApplicationCore.Features.Orders.OrderLoading.Dialog;

namespace ApplicationCore.Features.Orders.OrderLoading.Models;

public interface IOrderProvider {

    public IOrderLoadWidgetViewModel? OrderLoadingViewModel { get; set; }

    public Task<OrderData?> LoadOrderData(string source);

}
