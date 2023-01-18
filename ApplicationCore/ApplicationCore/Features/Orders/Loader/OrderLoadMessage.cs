using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.Orders.Loader;

public record OrderLoadMessage {

    public required string Message { get; init; }

    public required MessageSeverity Severity { get; init; }

}
