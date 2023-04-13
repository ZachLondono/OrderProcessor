using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApplicationCore.Infrastructure.Bus;

internal class CachedBusDecorator : IBus {

    private readonly ILogger<CachedBusDecorator> _logger;
    private readonly MediatRBus _bus;
    private readonly Dictionary<IBaseDomainRequest, CachedResponse> _cachedResponses = new();
    private readonly CacheConfiguration _configuration;

    public CachedBusDecorator(ILogger<CachedBusDecorator> logger, MediatRBus bus, IOptions<CacheConfiguration> configuration) {
        _logger = logger;
        _bus = bus;
        _configuration = configuration.Value;
    }

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : IDomainNotification {
        return _bus.Publish(notification, cancellationToken);
    }

    public async Task<Response<TResponse>> Send<TResponse>(IQuery<TResponse> request, CancellationToken cancellationToken = default) {

        if (typeof(TResponse) == typeof(Unit)) return await _bus.Send(request, cancellationToken);

        if (_cachedResponses.ContainsKey(request)) {

            var cachedResponse = _cachedResponses[request];

            if (DateTime.Now - cachedResponse.RecievedTimestamp < TimeSpan.FromSeconds(_configuration.TimeAlive)) {
                _logger.LogTrace("Returning cached response");
                return (Response<TResponse>)cachedResponse.Response;
            } else {
                _cachedResponses.Remove(request);
                _logger.LogTrace("Cached response is expired");
            }

        }

        var response = await _bus.Send(request, cancellationToken);

        if (_configuration.MaxCacheSize != 0 && _cachedResponses.Count > _configuration.MaxCacheSize) {
            var oldestRequest = _cachedResponses.OrderBy(kv => kv.Value.RecievedTimestamp).First();
            _cachedResponses.Remove(oldestRequest.Key);
            _logger.LogTrace("Removing oldest cached response {CachedRequest}", oldestRequest);
        }

        _cachedResponses.Add(request, new(response, DateTime.Now));
        _logger.LogTrace("Caching new response");

        return response;

    }

    public async Task<Response> Send(ICommand request, CancellationToken cancellationToken = default) {
        return await _bus.Send(request, cancellationToken);
    }

    public async Task<Response<TResquest>> Send<TResquest>(ICommand<TResquest> request, CancellationToken cancellationToken = default) {
        return await _bus.Send(request, cancellationToken);
    }
}