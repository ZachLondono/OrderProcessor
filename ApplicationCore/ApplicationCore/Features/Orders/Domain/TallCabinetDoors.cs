using ApplicationCore.Features.Orders.Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.Domain;

public record TallCabinetDoors {

    public int UpperQuantity { get; init; }
    public int LowerQuantity { get; init; }
    public double LowerDoorHeight { get; init; }
    public HingeSide HingeSide { get; init; } = HingeSide.NotApplicable;
    public MDFDoorOptions? MDFOptions { get; init; }

    public TallCabinetDoors(HingeSide hingeSide = HingeSide.NotApplicable, MDFDoorOptions? mdfOptions = null) {
        
        if (hingeSide == HingeSide.NotApplicable) { 
            UpperQuantity = 0;
            LowerQuantity = 2;
        } else {
            UpperQuantity = 0;
            LowerQuantity = 1;
        }

        HingeSide = hingeSide;
        MDFOptions = mdfOptions;

    }

    public TallCabinetDoors(double lowerDoorHeight, HingeSide hingeSide = HingeSide.NotApplicable, MDFDoorOptions? mdfOptions = null) {

        if (hingeSide == HingeSide.NotApplicable) {
            UpperQuantity = 2;
            LowerQuantity = 2;
        } else {
            UpperQuantity = 1;
            LowerQuantity = 1;
        }

        LowerDoorHeight = lowerDoorHeight;
        HingeSide = hingeSide;
        MDFOptions = mdfOptions;

    }

}
