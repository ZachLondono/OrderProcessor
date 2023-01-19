namespace ApplicationCore.Features.Orders.Loader.Dialog;

public record OrderLoadMessage {

    public required string Message { get; init; }

    public required MessageSeverity Severity { get; init; }

}
