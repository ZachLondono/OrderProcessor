using ApplicationCore.Infrastructure.UI;
namespace ApplicationCore.Features.CNC.GCode.Contracts.Notifications;

public record GCodeGenerationError(string Message) : IUINotification;