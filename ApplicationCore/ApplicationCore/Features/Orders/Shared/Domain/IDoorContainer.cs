using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.Shared.Domain;

/// <summary>
/// An object which contains doors
/// </summary>
internal interface IDoorContainer {

    IEnumerable<MDFDoor> GetDoors(Func<MDFDoorBuilder> getBuilder);

    bool ContainsDoors();

}
