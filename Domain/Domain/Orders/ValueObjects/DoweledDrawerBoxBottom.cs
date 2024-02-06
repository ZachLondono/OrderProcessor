using Domain.ValueObjects;

namespace Domain.Orders.ValueObjects;

public record DoweledDrawerBoxBottom(int ProductNumber, int Qty, Dimension Width, Dimension Length, DoweledDrawerBoxMaterial Material);