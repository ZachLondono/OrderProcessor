using ApplicationCore.Features.Orders.Shared.Domain.Enums;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public record CabinetFinishMaterial(string Finish, CabinetMaterialCore Core, string? PaintColor = null);
