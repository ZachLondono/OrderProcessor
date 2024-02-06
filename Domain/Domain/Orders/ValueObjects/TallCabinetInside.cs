using Domain.Orders.Enums;
using Domain.ValueObjects;

namespace Domain.Orders.ValueObjects;

public record TallCabinetInside {

    public int AdjustableShelvesLower { get; }
    public int AdjustableShelvesUpper { get; }
    public int VerticalDividersLower { get; }
    public int VerticalDividersUpper { get; }
    public RollOutOptions RollOutBoxes { get; }

    public static TallCabinetInside Empty() => new(0, 0, 0, 0, new(Array.Empty<Dimension>(), false, RollOutBlockPosition.None));

    public TallCabinetInside() {
        AdjustableShelvesUpper = 0;
        VerticalDividersLower = 0;
        VerticalDividersUpper = 0;
        RollOutBoxes = new(Array.Empty<Dimension>(), false, RollOutBlockPosition.None);
    }

    public TallCabinetInside(int adjShelvesUpper, int adjShelvesLower, int verticalDividersUpper, RollOutOptions rollOuts) {
        AdjustableShelvesLower = adjShelvesLower;
        AdjustableShelvesUpper = adjShelvesUpper;
        VerticalDividersLower = 0;
        VerticalDividersUpper = verticalDividersUpper;
        RollOutBoxes = rollOuts;
    }

    public TallCabinetInside(int adjShelvesUpper, int adjShelvesLower, int verticalDividersUpper, int verticalDividersLower) {
        AdjustableShelvesLower = adjShelvesLower;
        AdjustableShelvesUpper = adjShelvesUpper;
        VerticalDividersLower = verticalDividersLower;
        VerticalDividersUpper = verticalDividersUpper;
        RollOutBoxes = new(Array.Empty<Dimension>(), false, RollOutBlockPosition.None);
    }

    public TallCabinetInside(int adjShelvesUpper, int adjShelvesLower, int verticalDividersUpper, int verticalDividersLower, RollOutOptions rollOuts) {
        AdjustableShelvesLower = adjShelvesLower;
        AdjustableShelvesUpper = adjShelvesUpper;
        VerticalDividersLower = verticalDividersLower;
        VerticalDividersUpper = verticalDividersUpper;
        RollOutBoxes = rollOuts;

        if (VerticalDividersLower > 0 && RollOutBoxes.Any()) {
            throw new InvalidOperationException("Tall cabinet cannot contain roll out drawer boxes and vertical dividers in lower section");
        }
    }

}