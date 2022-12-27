using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CNC.GCode.Contracts.Notifications;

public record GCodeGenerationError(string Message) : IUINotification;