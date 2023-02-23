using ApplicationCore.Features.Orders.Shared.Domain.Products;

namespace ApplicationCore.Features.Orders.Shared.State.DataModels;

internal interface IProductDataModel {

    public IProduct MapToProduct();

}
