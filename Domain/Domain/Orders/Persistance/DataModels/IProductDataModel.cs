using Domain.Orders.Entities.Products;

namespace Domain.Orders.Persistance.DataModels;

public interface IProductDataModel {

    public IProduct MapToProduct();

}
