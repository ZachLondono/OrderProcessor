using Domain.Orders.Entities.Hardware;

namespace Domain.Orders.Entities.Products;

public interface ISupplyContainer {

    public IEnumerable<Supply> GetSupplies();

}
