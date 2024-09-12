using Domain.Orders.Entities;
using Domain.Orders.Entities.Products;

namespace Domain.Orders.Persistance.DataModels;

public interface IProductDataModel {

    public Guid Id { get; }

    public IProduct MapToProduct(IEnumerable<ProductionNote> productionNotes);

}
