using ApplicationCore.Features.Orders.Shared.Domain.Entities;

namespace ApplicationCore.Features.Orders.Details.OrderExport.Handlers;

internal class DovetailOrderHandler {

    public Task Handle(Order order, string template, string outputDirectory) => Task.CompletedTask;

}
