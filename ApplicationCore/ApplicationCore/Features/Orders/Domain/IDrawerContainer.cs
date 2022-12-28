namespace ApplicationCore.Features.Orders.Domain;

/// <summary>
/// An object which contains drawer boxes
/// </summary>
internal interface IDrawerContainer {

    IEnumerable<DrawerBox> GetDrawerBoxes();

}
