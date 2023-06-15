using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Shared;

namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.PackingList;

public interface IPackingListDecorator : IDocumentDecorator {

    public Task AddData(Order order);

}

