namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public class SupplyOptions {

    public bool IncludeHinges { get; set; } = true;
    public bool IncludeSlides { get; set; } = true;
    public bool IncludeShelfPins { get; set; } = true;
    public bool IncludeCams { get; set; } = true;
    public bool IncludeCabinetLegs { get; set; } = true;

}
