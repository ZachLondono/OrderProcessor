using Domain.ValueObjects;

namespace Domain.Orders.ValueObjects;

public record DoweledDrawerBoxMaterial(string Name, Dimension Thickness, bool IsGrained) {

    public PSIMaterial ToPSIMaterial() {
        return new PSIMaterial(Name, "MELA", Name, "MELA", "PB", Thickness.AsMillimeters());
    }

}