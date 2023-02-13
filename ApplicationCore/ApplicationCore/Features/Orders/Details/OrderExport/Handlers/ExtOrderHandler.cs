using ApplicationCore.Features.Orders.Shared.Domain.Entities;

namespace ApplicationCore.Features.Orders.Details.OrderExport.Handlers;

internal class ExtOrderHandler {

    public Task Handle(Order order, string outputDirectory) => Task.CompletedTask;

}