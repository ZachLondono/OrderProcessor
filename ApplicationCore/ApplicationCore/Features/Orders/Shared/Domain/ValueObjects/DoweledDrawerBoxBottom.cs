using Domain.ValueObjects;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public record DoweledDrawerBoxBottom(int ProductNumber, int Qty, Dimension Width, Dimension Length, DoweledDrawerBoxMaterial Material);