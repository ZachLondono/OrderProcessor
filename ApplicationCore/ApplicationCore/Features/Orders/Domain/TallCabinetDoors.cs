using ApplicationCore.Features.Orders.Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.Domain;

public record TallCabinetDoors {

    public int UpperQuantity { get; init; }
    public int LowerQuantity { get; init; }
    public HingeSide HingeSide { get; init; } = HingeSide.NotApplicable;
    public MDFDoorOptions? MDFOptions { get; init; }

    public TallCabinetDoors(bool twoSections, HingeSide hingeSide = HingeSide.NotApplicable, MDFDoorOptions? mdfOptions = null) {
        
        if (hingeSide == HingeSide.NotApplicable) { 
            UpperQuantity = twoSections ? 2 : 0;
            LowerQuantity = 2;
        } else {
            UpperQuantity = twoSections ? 1 : 0;
            LowerQuantity = 1;
        }

        HingeSide = hingeSide;
        MDFOptions = mdfOptions;

    }

}
