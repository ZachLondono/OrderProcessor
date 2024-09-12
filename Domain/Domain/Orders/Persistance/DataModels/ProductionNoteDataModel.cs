using Domain.Orders.Entities;

namespace Domain.Orders.Persistance.DataModels;

public class ProductionNoteDataModel {

    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public string Value { get; set; } = string.Empty;

    public ProductionNote ToDomainModel() => new(Id, Value);

    public static string GetQueryByProductId() => "SELECT id, product_id, value FROM product_production_notes WHERE product_id = @ProductId";

}
