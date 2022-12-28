using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;
using System.Security.AccessControl;

namespace ApplicationCore.Features.Orders.Domain.Products;

internal class BaseCabinet : Cabinet, IProduct {

    public BaseCabinetDoors Doors { get; }
    public IToeType ToeType { get; }
    public HorizontalDrawerBank Drawers { get; }
    public BaseCabinetInside Inside { get; }
    public BaseCabinetConstructionParameters Construction { get; }

    public BaseCabinet(int qty, decimal unitPrice,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetMaterial finishMaterial,
                        CabinetSide rightSide, CabinetSide leftSide,
                        BaseCabinetDoors doors, IToeType toeType, HorizontalDrawerBank drawers, BaseCabinetInside inside, BaseCabinetConstructionParameters construction)
                        : base(qty, unitPrice, height, width, depth, boxMaterial, finishMaterial, rightSide, leftSide) {
        Doors = doors;
        ToeType = toeType;
        Construction = construction;

        if (drawers.Quantity > 2)
            throw new InvalidOperationException("Base cabinet cannot have more than 2 drawers");

        if (drawers.GetBoxWidth(Width - 2 * Construction.MaterialThickness, Construction.MaterialThickness, Construction.GetSlideWidthAdjustment) < Dimension.Zero)
            throw new InvalidOperationException("Cannot fit drawerboxes in cabinet");

        if (drawers.FaceHeight > Height)
            throw new InvalidOperationException("Invalid drawer face size");
        Drawers = drawers;

        if (inside.RollOutBoxes.Positions.Length > 3)
            throw new InvalidOperationException("Base cabinet cannot have more than 3 roll out drawer boxes");

        foreach (var position in inside.RollOutBoxes.Positions) {
            if (position < Dimension.Zero || position > height - drawers.FaceHeight) {
                throw new InvalidOperationException("Roll out box position {position} is invalid for cabinet size");
            }
        }
        Inside = inside;
    }
}
