using Domain.Orders.Entities.Hardware;

namespace Domain.Orders.Entities.Products;

public interface IHangingRailContainer {

    public IEnumerable<HangingRail> GetHangingRails();

}
