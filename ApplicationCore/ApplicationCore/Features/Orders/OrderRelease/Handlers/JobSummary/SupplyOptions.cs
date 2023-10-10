using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.JobSummary;

public class SupplyOptions {

    public bool IncludeMiscellaneous { get; set; } = true;
    public bool IncludeHinges { get; set; } = true;
    public bool IncludeDoorPulls { get; set; } = true;
    public bool IncludeSlides { get; set; } = true;
    public bool IncludeShelfPins { get; set; } = true;
    public bool IncludeCams { get; set; } = true;
    public bool IncludeCabinetLegs { get; set; } = true;

    public void NoSupplies() {
        IncludeMiscellaneous = false;
        IncludeHinges = false;
        IncludeDoorPulls = false;
        IncludeSlides = false;
        IncludeShelfPins = false;
        IncludeCams = false;
        IncludeCabinetLegs = false;
    } 

    public void AllSupplies() {
        IncludeMiscellaneous = true;
        IncludeHinges = true;
        IncludeDoorPulls = true;
        IncludeSlides = true;
        IncludeShelfPins = true;
        IncludeCams = true;
        IncludeCabinetLegs = true;
    }

    public bool IncludeSupply(SupplyType supplyType) => supplyType switch {
        SupplyType.Miscellaneous => IncludeMiscellaneous,
        SupplyType.Hinges => IncludeHinges,
        SupplyType.Pulls => IncludeDoorPulls,
        SupplyType.DrawerSlides => IncludeSlides,
        SupplyType.ShelfPins => IncludeShelfPins,
        SupplyType.Cams => IncludeCams,
        SupplyType.CabinetLegs => IncludeCabinetLegs,
        _ => throw new InvalidOperationException("Unexpected supply type")
    };

}
