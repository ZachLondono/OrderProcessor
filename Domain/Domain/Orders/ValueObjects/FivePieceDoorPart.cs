using Domain.ValueObjects;

namespace Domain.Orders.ValueObjects;

public record FivePieceDoorPart(string Name, int Qty, Dimension Width, Dimension Length, string Material);
