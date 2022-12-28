namespace ApplicationCore.Features.Orders.Domain;

public record BaseCabinetDoors {

    public int Quantity { get; init; }
    public HingeSide HingeSide { get; init; } = HingeSide.NotApplicable;
    
    public BaseCabinetDoors() {
        Quantity = 2;
        HingeSide = HingeSide.NotApplicable;
    }

    public BaseCabinetDoors(HingeSide hingeSide) {
        Quantity = 1;
        HingeSide = hingeSide;
    }

}
