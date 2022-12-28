using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Domain.Products;

internal class BaseCabinet : Cabinet {

    public BaseCabinetDoors Doors { get; }
    public IToeType ToeType { get; }
    public HorizontalDrawerBank Drawers { get; }
    public BaseCabinetInside Inside { get; }

    public static BaseCabinet Create(int qty, decimal unitPrice, string room,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetMaterial finishMaterial,
                        CabinetSide rightSide, CabinetSide leftSide,
                        BaseCabinetDoors doors, IToeType toeType, HorizontalDrawerBank drawers, BaseCabinetInside inside) {
        return new(Guid.NewGuid(), qty, unitPrice, room, height, width, depth, boxMaterial, finishMaterial, rightSide, leftSide, doors, toeType, drawers, inside);
    }

    private BaseCabinet(Guid id, int qty, decimal unitPrice, string room,
                        Dimension height, Dimension width, Dimension depth,
                        CabinetMaterial boxMaterial, CabinetMaterial finishMaterial,
                        CabinetSide rightSide, CabinetSide leftSide,
                        BaseCabinetDoors doors, IToeType toeType, HorizontalDrawerBank drawers, BaseCabinetInside inside)
                        : base(id, qty, unitPrice, room, height, width, depth, boxMaterial, finishMaterial, rightSide, leftSide) {
        Doors = doors;
        ToeType = toeType;
        
        if (doors == BaseCabinetDoors.OneDoor && drawers.Quantity > 1)
            throw new InvalidOperationException("Base cabinet cannot have more than 1 drawer if it only has 1 door");

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

    public override Dictionary<string, string> GetParameters() {
        var parameters = new Dictionary<string, string>() {
            { "ProductW", Width.AsMillimeters().ToString() },
            { "ProductH", Height.AsMillimeters().ToString() },
            { "ProductD", Depth.AsMillimeters().ToString() },
            { "FinishedLeft", "0" },
            { "FinishedRight", "0" },
            { "ShelfQ", "0" },
            { "DividerQ", "0" },
            { "Rollout1", "0" },
            { "Rollout2", "0" },
            { "Rollout3", "0" },
            { "PulloutBlockType", "0" },
            { "AppliedPanel", "0" },
        };

        if (Doors == BaseCabinetDoors.OneDoor) {
            parameters.Add("HingeLeft", "0");
        }

        return parameters;
    }

    public override string GetProductName() {
        
        if (Doors == BaseCabinetDoors.OneDoor) {

            if (Drawers.Quantity == 1) return "B1D1D";
            return "B1D";

        }

        if (Drawers.Quantity == 2) return "B2D2D";
        else if (Drawers.Quantity == 2) return "B2D2D";
        return "B2D";

    }
}
