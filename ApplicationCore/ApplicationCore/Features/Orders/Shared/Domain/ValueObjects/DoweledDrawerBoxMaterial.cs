using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public record DoweledDrawerBoxMaterial(string Name, Dimension Thickness, bool IsGrained);