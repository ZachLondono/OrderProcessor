using ApplicationCore.Features.CNC.ReleasePDF;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public record DoweledDrawerBoxMaterial(string Name, Dimension Thickness, bool IsGrained) {

    public PSIMaterial ToPSIMaterial() {
        return new PSIMaterial(Name, "MELA", Name, "MELA", "PB", Thickness.AsMillimeters());
    }

}