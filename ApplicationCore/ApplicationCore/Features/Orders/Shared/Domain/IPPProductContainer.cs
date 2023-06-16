using ApplicationCore.Features.Orders.OrderExport.Handlers.ExtExport.Contracts;

namespace ApplicationCore.Features.Orders.Shared.Domain;

internal interface IPPProductContainer {

    IEnumerable<PPProduct> GetPPProducts();

    bool ContainsPPProducts();

}
