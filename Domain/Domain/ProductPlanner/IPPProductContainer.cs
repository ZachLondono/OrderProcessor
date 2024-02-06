namespace Domain.ProductPlanner;

public interface IPPProductContainer {

    IEnumerable<PPProduct> GetPPProducts();

    bool ContainsPPProducts();

}
