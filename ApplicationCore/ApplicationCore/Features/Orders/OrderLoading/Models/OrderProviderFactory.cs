using ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.LoadAllmoxyFileOrderData;
using ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.LoadAllmoxyWebOrderData;
using ApplicationCore.Features.Orders.OrderLoading.LoadClosetOrderSpreadsheetOrderData;
using ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.LoadClosetProFileOrderData;
using ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.LoadClosetProWebOrderData;
using ApplicationCore.Features.Orders.OrderLoading.LoadDoorSpreadsheetOrderData;
using ApplicationCore.Features.Orders.OrderLoading.LoadDoweledDBSpreadsheetOrderData;
using Microsoft.Extensions.DependencyInjection;

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
        _ => throw new KeyNotFoundException("No valid order provider for given order source type"),
    };

}
