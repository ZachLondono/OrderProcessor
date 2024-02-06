using ApplicationCore.Features.Orders.OrderExport.Handlers.ExtExport.Contracts;

namespace Domain.Orders;

internal interface IPPProductContainer {

    IEnumerable<PPProduct> GetPPProducts();

    bool ContainsPPProducts();

}
