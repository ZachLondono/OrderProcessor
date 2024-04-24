using Domain.ValueObjects;

namespace Domain.Orders.Entities.Hardware;

public record HangingRail(Guid Id, int Qty, Dimension Length, string Finish);
