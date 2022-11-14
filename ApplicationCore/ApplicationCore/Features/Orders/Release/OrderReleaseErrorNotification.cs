using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.Orders.Release;

public record OrderReleaseErrorNotification(string Message) : IUINotification;
