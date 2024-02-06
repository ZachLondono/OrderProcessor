using Domain.ValueObjects;

namespace Domain.Orders.ValueObjects;

public record AdditionalOpening(Dimension RailWidth, Dimension OpeningHeight);
