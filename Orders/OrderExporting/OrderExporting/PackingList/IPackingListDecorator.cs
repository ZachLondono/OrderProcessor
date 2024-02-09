using Domain.Orders.Entities;
using OrderExporting.Shared;

namespace OrderExporting.PackingList;

public interface IPackingListDecorator : IDocumentDecorator {

    public bool IncludeCheckBoxesNextToItems { get; set; }
    public bool IncludeSignatureField { get; set; }

    public Task AddData(Order order);

}
