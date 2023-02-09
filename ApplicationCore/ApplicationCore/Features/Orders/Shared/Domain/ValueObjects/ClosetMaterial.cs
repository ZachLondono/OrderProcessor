using ApplicationCore.Features.Orders.Shared.Domain.Enums;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public record ClosetMaterial(string Finish, ClosetMaterialCore Core, string? PaintColor);