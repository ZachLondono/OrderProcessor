using ApplicationCore.Features.Orders.OrderLoading.Dialog;
using ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData.Models;
using ApplicationCore.Features.Orders.OrderLoading.Models;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadClosetProOrderData;

internal class ClosetProCSVOrderProvider : IOrderProvider {

    public IOrderLoadWidgetViewModel? OrderLoadingViewModel { get; set; }

    private readonly ILogger<ClosetProCSVOrderProvider> _logger;
    private readonly ClosetProCSVReader _reader;
    private readonly ClosetProPartMapper _partMapper;

    public ClosetProCSVOrderProvider(ILogger<ClosetProCSVOrderProvider> logger, ClosetProCSVReader reader, ClosetProPartMapper partMapper) {
        _logger = logger;
        _reader = reader;
        _partMapper = partMapper;
    }

    public Task<OrderData?> LoadOrderData(string source) {

        if (!File.Exists(source)) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "File cannot be found");
            return Task.FromResult((OrderData?)null);
        }

        _reader.OnReadError += (msg) => OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, msg);

        var info = _reader.ReadCSVFile(source);

        List<IProduct> products = new();
        foreach (var part in info.Parts) {
            var product = _partMapper.CreateProductFromPart(part);
            if (product is not null) {
                products.Add(product);
            } else {
                OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Warning, $"Skipping part {part.PartNum} - {part.PartName} / {part.ExportName}");
                _logger.LogWarning("Skipping part {Part}", part);
            }
        }

        throw new NotImplementedException();

    }
}
