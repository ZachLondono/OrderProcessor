using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.Orders.Shared.Domain.Notifications;

internal record OrderCreatedNotification : IDomainNotification {

    public required Order Order { get; init; }

}
