using ApplicationCore.Features.Companies.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.Orders.Complete;

public record TriggerOrderCompleteNotification(Order Order, CompleteProfile CompleteProfile) : IDomainNotification;
