using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;

public record AdditionalOpening(Dimension RailWidth, Dimension OpeningHeight);
