using Domain.Orders.Builders;
using Domain.Orders.Components;

namespace Domain.Orders;

public interface IMDFDoorContainer {

    IEnumerable<MDFDoor> GetDoors(Func<MDFDoorBuilder> getBuilder);

    bool ContainsDoors();

}
