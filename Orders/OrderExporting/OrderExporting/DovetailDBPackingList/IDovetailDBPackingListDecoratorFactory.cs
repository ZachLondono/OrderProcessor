using Domain.Orders.Entities;

namespace OrderExporting.DovetailDBPackingList;

public interface IDovetailDBPackingListDecoratorFactory {
    Task<DovetailDBPackingListDecorator> CreateDecorator(Order order);
}