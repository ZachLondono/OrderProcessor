using OrderLoading.LoadAllmoxyOrderData.LoadAllmoxyFileOrderData;
using OrderLoading.LoadAllmoxyOrderData.LoadAllmoxyWebOrderData;
using OrderLoading.LoadClosetOrderSpreadsheetOrderData;
using OrderLoading.LoadClosetProOrderData.LoadClosetProFileOrderData;
using OrderLoading.LoadDoorSpreadsheetOrderData;
using OrderLoading.LoadDoweledDBSpreadsheetOrderData;
using OrderLoading.LoadHafeleDBSpreadsheetOrderData;
using Microsoft.Extensions.DependencyInjection;
using OrderLoading.LoadClosetProOrderData.LoadClosetProWebOrderData;
using OrderLoading;

namespace ApplicationCore.Features.Orders.OrderLoading.Models;

public class OrderProviderFactory : IOrderProviderFactory {

    private readonly IServiceProvider _serviceProvider;

    public OrderProviderFactory(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
    }

    public IOrderProvider GetOrderProvider(OrderSourceType source) => source switch {
        OrderSourceType.AllmoxyWebXML => _serviceProvider.GetRequiredService<AllmoxyWebXMLOrderProvider>(),
        OrderSourceType.AllmoxyFileXML => _serviceProvider.GetRequiredService<AllmoxyFileXMLOrderProvider>(),
        OrderSourceType.ClosetProFileCSV => _serviceProvider.GetRequiredService<ClosetProFileCSVOrderProvider>(),
        OrderSourceType.ClosetProWebCSV => _serviceProvider.GetRequiredService<ClosetProWebCSVOrderProvider>(),
        OrderSourceType.DoorOrder => _serviceProvider.GetRequiredService<DoorSpreadsheetOrderProvider>(),
        OrderSourceType.DoweledDBOrderForm => _serviceProvider.GetRequiredService<DoweledDBSpreadsheetOrderProvider>(),
        OrderSourceType.ClosetOrderForm => _serviceProvider.GetRequiredService<ClosetSpreadsheetOrderProvider>(),
        OrderSourceType.HafeleDBOrderForm => _serviceProvider.GetRequiredService<HafeleDBSpreadSheetOrderProvider>(),
        _ => throw new KeyNotFoundException("No valid order provider for given order source type"),
    };

}
