using ApplicationCore.Features.Companies.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.Orders.Release;

public record TriggerOrderReleaseNotification(Order Order, ReleaseProfile ReleaseProfile) : IDomainNotification;
