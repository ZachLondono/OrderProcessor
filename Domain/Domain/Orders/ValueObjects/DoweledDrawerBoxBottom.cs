using Domain.ValueObjects;

namespace Domain.Orders.ValueObjects;

public record DoweledDrawerBoxBottom(int Qty, Dimension Width, Dimension Length, DoweledDrawerBoxMaterial Material);