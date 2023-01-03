using ApplicationCore.Features.ProductPlanner.Contracts;

namespace ApplicationCore.Features.Orders.Domain;

internal interface IPPProductContainer {

    IEnumerable<PPProduct> GetPPProducts();

}