using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Shared;

namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.JobSummary;

public interface IJobSummaryDecorator : IDocumentDecorator {

    public Task AddData(Order order, bool showItems, SupplyOptions supplyOptions, bool showInvoiceSummary, string[] materialTypes, bool showMaterialTypes);

}
