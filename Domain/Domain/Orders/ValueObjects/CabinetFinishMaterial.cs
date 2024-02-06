using Domain.Orders.Enums;
using System.Diagnostics.CodeAnalysis;

namespace Domain.Orders.ValueObjects;

public record CabinetFinishMaterial {

    public required string Finish { get; init; }
    public required CabinetMaterialFinishType FinishType { get; init; }
    public required CabinetMaterialCore Core { get; init; }
    public required string? PaintColor { get; init; }

    private CabinetFinishMaterial() { }

    [SetsRequiredMembers]
    public CabinetFinishMaterial(string finish, CabinetMaterialFinishType finishType, CabinetMaterialCore core, string? paintColor = null) {
        Finish = finish;
        FinishType = finishType;
        Core = core;
        PaintColor = paintColor;

        if (paintColor is not null && finishType != CabinetMaterialFinishType.Paint) {
            throw new ArgumentException("Paint color was set but finish type is not set to paint");
        }
    }

    public static CabinetFinishMaterial UnPaintedMelaPB(string finish) => new() {
        Finish = finish,
        FinishType = CabinetMaterialFinishType.Melamine,
        Core = CabinetMaterialCore.ParticleBoard,
        PaintColor = null
    };

    public static CabinetFinishMaterial PaintedPB(string matFinish, string paintColor) => new() {
        Finish = matFinish,
        FinishType = CabinetMaterialFinishType.Paint,
        Core = CabinetMaterialCore.ParticleBoard,
        PaintColor = paintColor
    };

    public static CabinetFinishMaterial UnPaintedVeneerPly(string finish) => new() {
        Finish = finish,
        FinishType = CabinetMaterialFinishType.Veneer,
        Core = CabinetMaterialCore.Plywood,
        PaintColor = null
    };

    public static CabinetFinishMaterial PaintedPly(string matFinish, string paintColor) => new() {
        Finish = matFinish,
        FinishType = CabinetMaterialFinishType.Paint,
        Core = CabinetMaterialCore.Plywood,
        PaintColor = paintColor
    };

}
