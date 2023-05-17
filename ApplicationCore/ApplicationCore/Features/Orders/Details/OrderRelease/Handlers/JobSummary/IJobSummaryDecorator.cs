using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Shared;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.JobSummary;

public interface IJobSummaryDecorator : IDocumentDecorator {

    public Task AddData(Order order, bool showItems, bool showSupplies);

}
