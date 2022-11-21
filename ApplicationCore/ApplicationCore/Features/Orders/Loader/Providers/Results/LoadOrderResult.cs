using ApplicationCore.Features.Orders.Loader.Providers.DTO;

namespace ApplicationCore.Features.Orders.Loader.Providers.Results;

public record LoadOrderResult {

    public required OrderData? Data { get; init; }

    public IEnumerable<LoadMessage> Messages { get; init; } = new List<LoadMessage>();

}
