using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain;

public record TallCabinetInside {

    public int AdjustableShelvesLower { get; }
    public int AdjustableShelvesUpper { get; }
    public int VerticalDividersLower { get; }
    public int VerticalDividersUpper { get; }
    public RollOutOptions RollOutBoxes { get; }

    public TallCabinetInside() {
        AdjustableShelvesUpper = 0;
        VerticalDividersLower = 0;
        VerticalDividersUpper = 0;
        RollOutBoxes = new(Array.Empty<Dimension>(), false, RollOutBlockPosition.None, DrawerSlideType.UnderMount, CabinetDrawerBoxMaterial.FingerJointBirch);
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
        RollOutBoxes = new(Array.Empty<Dimension>(), false, RollOutBlockPosition.None, DrawerSlideType.UnderMount, CabinetDrawerBoxMaterial.FingerJointBirch);
    }

}