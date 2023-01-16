using ApplicationCore.Features.Orders.Shared.Domain.Builders;

namespace ApplicationCore.Features.Orders.Shared.Domain;

/// <summary>
/// An object which contains doors
/// </summary>
internal interface IDoorContainer {

    IEnumerable<MDFDoor> GetDoors(Func<MDFDoorBuilder> getBuilder);

}
