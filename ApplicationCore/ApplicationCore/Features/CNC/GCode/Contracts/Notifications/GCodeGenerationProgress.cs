using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CNC.GCode.Contracts.Notifications;

public record GCodeGenerationProgress(double PercentComplete, string Message) : IUINotification;
