using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Domain;

public record TallCabinetDoors {

    public int UpperQuantity { get; init; }
    public int LowerQuantity { get; init; }
    public Dimension LowerDoorHeight { get; init; }
    public HingeSide HingeSide { get; init; } = HingeSide.NotApplicable;
    public MDFDoorOptions? MDFOptions { get; init; }

    public static TallCabinetDoors NoDoors() => new() {
        UpperQuantity = 0,
        LowerQuantity = 0,
        LowerDoorHeight = Dimension.Zero,
        HingeSide = HingeSide.NotApplicable,
        MDFOptions = null
    };

    public TallCabinetDoors(HingeSide hingeSide = HingeSide.NotApplicable, MDFDoorOptions? mdfOptions = null) {
        
        if (hingeSide == HingeSide.NotApplicable) { 
            UpperQuantity = 0;
            LowerQuantity = 2;
        } else {
            UpperQuantity = 0;
            LowerQuantity = 1;
        }

        LowerDoorHeight = Dimension.Zero;
        HingeSide = hingeSide;
        MDFOptions = mdfOptions;

    }

    public TallCabinetDoors(Dimension lowerDoorHeight, HingeSide hingeSide = HingeSide.NotApplicable, MDFDoorOptions? mdfOptions = null) {

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
