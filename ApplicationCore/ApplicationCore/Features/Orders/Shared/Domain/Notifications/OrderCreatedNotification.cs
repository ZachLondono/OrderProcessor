using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.Orders.Shared.Domain.Notifications;

internal record OrderCreatedNotification : IDomainNotification {

    public required Order Order { get; init; }

}
