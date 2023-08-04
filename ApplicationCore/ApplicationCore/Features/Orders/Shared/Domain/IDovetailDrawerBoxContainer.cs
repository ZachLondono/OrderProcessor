using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Components;

namespace ApplicationCore.Features.Orders.Shared.Domain;

/// <summary>
/// An object which contains dovetail drawer boxes
/// </summary>
internal interface IDovetailDrawerBoxContainer {

    IEnumerable<DovetailDrawerBox> GetDovetailDrawerBoxes(Func<DovetailDrawerBoxBuilder> getBuilder);

    bool ContainsDovetailDrawerBoxes();

}
