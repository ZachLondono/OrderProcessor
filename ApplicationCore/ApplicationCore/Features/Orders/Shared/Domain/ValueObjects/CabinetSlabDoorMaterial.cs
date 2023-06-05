using ApplicationCore.Features.Orders.Shared.Domain.Enums;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public record CabinetSlabDoorMaterial(string Finish, CabinetMaterialFinishType FinishType, CabinetMaterialCore Core, string? PaintColor = null);
