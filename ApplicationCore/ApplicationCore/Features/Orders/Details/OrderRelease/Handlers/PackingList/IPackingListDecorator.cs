using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Shared;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.PackingList;

public interface IPackingListDecorator : IDocumentDecorator {

    public Task AddData(Order order);

}

