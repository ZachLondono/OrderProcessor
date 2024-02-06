namespace Domain.Orders.ValueObjects;

public record WallCabinetInside {

    public int AdjustableShelves { get; }
    public int VerticalDividers { get; }

    public static WallCabinetInside Empty() => new WallCabinetInside(0, 0);

    public WallCabinetInside() {
        AdjustableShelves = 0;
        VerticalDividers = 0;
    }

    public WallCabinetInside(int adjShelves, int verticalDividers) {
        AdjustableShelves = adjShelves;
        VerticalDividers = verticalDividers;
    }

}
