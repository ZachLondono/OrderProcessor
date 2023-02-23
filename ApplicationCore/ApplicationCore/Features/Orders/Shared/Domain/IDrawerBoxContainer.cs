using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.Shared.Domain;

/// <summary>
/// An object which contains drawer boxes
/// </summary>
internal interface IDrawerBoxContainer {

    DrawerBoxOptions DrawerBoxOptions { get; }

    IEnumerable<DovetailDrawerBox> GetDrawerBoxes(Func<DovetailDrawerBoxBuilder> getBuilder);

}
