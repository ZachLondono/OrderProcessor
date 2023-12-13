using ApplicationCore.Features.Orders.OrderLoading.Dialog;
using ApplicationCore.Features.Orders.OrderLoading.Models;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadTailoredLivingSpreadsheetOrderData;

public class TailoredLivingOrderSpreadsheetOrderProvider : IOrderProvider {

    public IOrderLoadWidgetViewModel? OrderLoadingViewModel { get; set; }

    public Task<OrderData?> LoadOrderData(string source) {

        throw new NotImplementedException();

    }

}

