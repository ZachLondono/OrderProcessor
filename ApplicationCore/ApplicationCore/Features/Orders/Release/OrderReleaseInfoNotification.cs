using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.Orders.Release;

public record OrderReleaseInfoNotification(string Message) : IUINotification;
