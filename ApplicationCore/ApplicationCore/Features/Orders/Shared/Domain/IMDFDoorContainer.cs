using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Components;

namespace ApplicationCore.Features.Orders.Shared.Domain;

internal interface IMDFDoorContainer {

    IEnumerable<MDFDoor> GetDoors(Func<MDFDoorBuilder> getBuilder);

    bool ContainsDoors();

}
