using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.Orders.Release;

public record OrderReleaseProgressNotification(string Message) : IUINotification;
