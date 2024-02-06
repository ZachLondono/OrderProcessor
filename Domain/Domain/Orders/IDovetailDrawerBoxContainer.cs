using Domain.Orders.Builders;
using Domain.Orders.Components;

namespace Domain.Orders;

/// <summary>
/// An object which contains dovetail drawer boxes
/// </summary>
internal interface IDovetailDrawerBoxContainer {

    IEnumerable<DovetailDrawerBox> GetDovetailDrawerBoxes(Func<DovetailDrawerBoxBuilder> getBuilder);

    bool ContainsDovetailDrawerBoxes();

}
