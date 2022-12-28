using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Domain;

public record BaseCabinetInside {

    public int AdjustableShelves { get; }
    public int VerticalDividers { get; }
    public RollOutOptions RollOutBoxes { get; }

    public BaseCabinetInside() {
        AdjustableShelves = 0;
        VerticalDividers = 0;
        RollOutBoxes = new(Array.Empty<Dimension>(), false, RollOutBlockPosition.None, DrawerSlideType.UnderMount, CabinetDrawerBoxMaterial.FingerJointBirch);
    }

    public BaseCabinetInside(int adjShelves, RollOutOptions rollOuts) {
        AdjustableShelves = adjShelves;
        VerticalDividers = 0;
        RollOutBoxes = rollOuts;
    }

    public BaseCabinetInside(int adjShelves, int verticalDividers) {
        AdjustableShelves = adjShelves;
        VerticalDividers = verticalDividers;
        RollOutBoxes = new(Array.Empty<Dimension>(), false, RollOutBlockPosition.None, DrawerSlideType.UnderMount, CabinetDrawerBoxMaterial.FingerJointBirch);
    }

}
