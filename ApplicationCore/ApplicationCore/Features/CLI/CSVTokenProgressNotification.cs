using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.CLI;

public class CSVTokenProgressNotification : IUINotification {

	public string Message { get; init; }

	public CSVTokenProgressNotification(string message) {
		Message = message;
	}

}
