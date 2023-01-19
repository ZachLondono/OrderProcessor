namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public record WallCabinetInside {

    public int AdjustableShelves { get; }
    public int VerticalDividers { get; }

    public WallCabinetInside() {
        AdjustableShelves = 0;
        VerticalDividers = 0;
    }

    public WallCabinetInside(int adjShelves, int verticalDividers) {
        AdjustableShelves = adjShelves;
        VerticalDividers = verticalDividers;
    }

}
