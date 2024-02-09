using Domain.Orders.Entities;
using OrderExporting.Shared;

namespace OrderExporting.JobSummary;

public interface IJobSummaryDecorator : IDocumentDecorator {

    public Task AddData(Order order, bool showItems, SupplyOptions supplyOptions, string[] materialTypes, bool showMaterialTypes);

}
