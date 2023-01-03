namespace ApplicationCore.Features.Orders.Domain;

internal interface IPPProductContainer {

    IEnumerable<PPProduct> GetPPProducts();

}