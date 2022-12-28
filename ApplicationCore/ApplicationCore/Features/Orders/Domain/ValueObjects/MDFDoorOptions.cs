using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Domain.ValueObjects;

public record MDFDoorOptions(string StyleName, Dimension Rails, Dimension Stiles);
