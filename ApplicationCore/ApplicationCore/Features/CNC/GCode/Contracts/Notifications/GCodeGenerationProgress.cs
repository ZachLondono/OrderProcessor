using ApplicationCore.Infrastructure.UI;

namespace ApplicationCore.Features.CNC.GCode.Contracts.Notifications;

public record GCodeGenerationProgress(double PercentComplete, string Message) : IUINotification;
