using Domain.Orders.Entities;

namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.DovetailDBPackingList;
public interface IDovetailDBPackingListDecoratorFactory {
    Task<DovetailDBPackingListDecorator> CreateDecorator(Order order);
}