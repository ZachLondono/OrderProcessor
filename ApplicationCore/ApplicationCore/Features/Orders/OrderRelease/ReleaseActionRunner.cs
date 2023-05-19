using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Shared.Components.ProgressModal;

namespace ApplicationCore.Features.Orders.OrderRelease;

public class ReleaseActionRunner : IActionRunner {

        public Action<ProgressLogMessage>? PublishProgressMessage { get; set; }

        private readonly ReleaseService _service;
        private readonly Order _order;
        private readonly ReleaseConfiguration _configuration;

        public ReleaseActionRunner(ReleaseService service, Order order, ReleaseConfiguration configuration) {
            _service = service;
            _order = order;
            _configuration = configuration;

            _service.OnProgressReport += (msg) => PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Info, msg));
            _service.OnError += (msg) => PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Error, msg));
            _service.OnActionComplete += (msg) => PublishProgressMessage?.Invoke(new(ProgressLogMessageType.Success, msg));
            _service.OnFileGenerated += (msg) => PublishProgressMessage?.Invoke(new(ProgressLogMessageType.FileCreated, msg));
        }

        public async Task Run() {
            await _service.Release(_order, _configuration);
        }
    }
