using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Domain.Products;

internal class BaseCabinet : Cabinet {

    public BaseCabinetDoors Doors { get; }
    public IToeType ToeType { get; }
    public HorizontalDrawerBank Drawers { get; }
    public BaseCabinetInside Inside { get; }

    public static BaseCabinet Create(int qty, decimal unitPrice,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetMaterial finishMaterial,
                        CabinetSide rightSide, CabinetSide leftSide,
                        BaseCabinetDoors doors, IToeType toeType, HorizontalDrawerBank drawers, BaseCabinetInside inside) {
        return new(Guid.NewGuid(), qty, unitPrice, height, width, depth, boxMaterial, finishMaterial, rightSide, leftSide, doors, toeType, drawers, inside);
    }

    private BaseCabinet(Guid id, int qty, decimal unitPrice,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetMaterial finishMaterial,
                        CabinetSide rightSide, CabinetSide leftSide,
                        BaseCabinetDoors doors, IToeType toeType, HorizontalDrawerBank drawers, BaseCabinetInside inside)
                        : base(id, qty, unitPrice, height, width, depth, boxMaterial, finishMaterial, rightSide, leftSide) {
        Doors = doors;
        ToeType = toeType;
        
        if (drawers.Quantity > 2)
            throw new InvalidOperationException("Base cabinet cannot have more than 2 drawers");

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
