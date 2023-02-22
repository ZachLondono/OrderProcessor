using ApplicationCore.Features.Orders.Details.OrderExport.Handlers.ExtExport.Contracts;

namespace ApplicationCore.Features.Orders.Shared.Domain;

internal interface IPPProductContainer {

    IEnumerable<PPProduct> GetPPProducts();

}