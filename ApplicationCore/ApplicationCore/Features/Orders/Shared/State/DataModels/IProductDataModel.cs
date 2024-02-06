using Domain.Orders.Entities.Products;

namespace ApplicationCore.Features.Orders.Shared.State.DataModels;

internal interface IProductDataModel {

    public IProduct MapToProduct();

}
