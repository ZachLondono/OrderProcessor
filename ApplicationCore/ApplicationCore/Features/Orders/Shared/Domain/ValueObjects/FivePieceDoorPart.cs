using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public record FivePieceDoorPart(string Name, int Qty, Dimension Width, Dimension Length, string Material);
