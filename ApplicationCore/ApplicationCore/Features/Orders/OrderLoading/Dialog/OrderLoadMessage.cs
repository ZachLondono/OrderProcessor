namespace ApplicationCore.Features.Orders.OrderLoading.Dialog;

public record OrderLoadMessage {

    public required string Message { get; init; }

    public required MessageSeverity Severity { get; init; }

}
