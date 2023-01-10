namespace ApplicationCore.Features.Orders.Domain;

/// <summary>
/// An object which contains drawer boxes
/// </summary>
internal interface IDrawerBoxContainer {

    IEnumerable<DovetailDrawerBox> GetDrawerBoxes();

}
