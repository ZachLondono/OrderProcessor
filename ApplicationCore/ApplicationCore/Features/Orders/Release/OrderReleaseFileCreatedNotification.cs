using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.Orders.Release;

public record OrderReleaseFileCreatedNotification(string Message, string FilePath) : IUINotification;
