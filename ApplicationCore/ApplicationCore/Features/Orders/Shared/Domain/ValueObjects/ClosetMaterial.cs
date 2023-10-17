using ApplicationCore.Features.CNC.ReleasePDF;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public record ClosetMaterial(string Finish, ClosetMaterialCore Core) {

    public PSIMaterial ToPSIMaterial(Dimension thickness) {

        var materialCore = Core switch {
            ClosetMaterialCore.Plywood => "Ply",
            _ or ClosetMaterialCore.ParticleBoard => "PB"
        };

        return new PSIMaterial(Finish, "MELA", Finish, "MELA", materialCore, thickness.AsMillimeters());

    }

}