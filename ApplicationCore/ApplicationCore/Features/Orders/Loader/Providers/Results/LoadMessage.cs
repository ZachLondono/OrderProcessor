namespace ApplicationCore.Features.Orders.Loader.Providers.Results;

public record LoadMessage {

    public required string Message { get; init; }

    public required MessageSeverity Severity { get; init; }

}
