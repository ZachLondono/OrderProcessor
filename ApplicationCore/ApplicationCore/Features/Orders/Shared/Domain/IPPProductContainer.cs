using ApplicationCore.Features.ProductPlanner.Contracts;

namespace ApplicationCore.Features.Orders.Shared.Domain;

internal interface IPPProductContainer {

    IEnumerable<PPProduct> GetPPProducts();

}