using ApplicationCore.Features.Orders.OrderLoading.Dialog;
using ApplicationCore.Features.Orders.OrderLoading.Models;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadHafeleDBSpreadsheetOrderData;

internal class HafeleDBSpreadSheetOrderProvider : IOrderProvider {

    public IOrderLoadWidgetViewModel? OrderLoadingViewModel { get; set; }

    public HafeleDBSpreadSheetOrderProvider() {

    }

    public Task<OrderData?> LoadOrderData(string source) {
        throw new NotImplementedException();
    }

}
