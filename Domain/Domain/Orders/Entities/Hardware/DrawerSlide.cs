using Domain.ValueObjects;

namespace Domain.Orders.Entities.Hardware;

public record DrawerSlide(Guid Id, int Qty, Dimension Length, string Style);
