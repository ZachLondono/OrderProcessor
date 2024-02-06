using Domain.Orders.Enums;
using Domain.ValueObjects;

namespace Domain.Orders.ValueObjects;

public record ClosetMaterial(string Finish, ClosetMaterialCore Core) {

    public PSIMaterial ToPSIMaterial(Dimension thickness) {

        var materialCore = Core switch {
            ClosetMaterialCore.Plywood => "Ply",
            _ or ClosetMaterialCore.ParticleBoard => "PB"
        };

        return new PSIMaterial(Finish, "MELA", Finish, "MELA", materialCore, thickness.AsMillimeters());

    }

}