using ApplicationCore.Features.Orders.Shared.Domain.Enums;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public record CabinetMaterial(string Finish, CabinetMaterialCore Core);
