using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.Orders.Release;

public record OrderReleaseSuccessNotification(string Message) : IUINotification;
