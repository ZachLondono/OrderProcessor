using Domain.Orders.Enums;
using Domain.ValueObjects;

namespace Domain.Orders.ValueObjects;

public record BaseCabinetInside {

    public int AdjustableShelves { get; }
    public int VerticalDividers { get; }
    public RollOutOptions RollOutBoxes { get; }
    public ShelfDepth ShelfDepth { get; }

    public static BaseCabinetInside Empty() => new(0, 0, new(Array.Empty<Dimension>(), false, RollOutBlockPosition.None), ShelfDepth.Default);

    public BaseCabinetInside() {
        AdjustableShelves = 0;
        VerticalDividers = 0;
        RollOutBoxes = new(Array.Empty<Dimension>(), false, RollOutBlockPosition.None);
    }

    public BaseCabinetInside(int adjShelves, RollOutOptions rollOuts, ShelfDepth shelfDepth) {
        AdjustableShelves = adjShelves;
        VerticalDividers = 0;
        RollOutBoxes = rollOuts;
        ShelfDepth = shelfDepth;
    }

    public BaseCabinetInside(int adjShelves, int verticalDividers, ShelfDepth shelfDepth) {
        AdjustableShelves = adjShelves;
        VerticalDividers = verticalDividers;
        RollOutBoxes = new(Array.Empty<Dimension>(), false, RollOutBlockPosition.None);
        ShelfDepth = shelfDepth;
    }

    public BaseCabinetInside(int adjShelves, int verticalDividers, RollOutOptions rollOuts, ShelfDepth shelfDepth) {
        AdjustableShelves = adjShelves;
        VerticalDividers = verticalDividers;
        RollOutBoxes = rollOuts;
        ShelfDepth = shelfDepth;

        if (VerticalDividers > 0 && RollOutBoxes.Any()) {
            throw new InvalidOperationException("Base cabinet cannot contain vertical dividers and roll out drawer boxes");
        }

    }

}
