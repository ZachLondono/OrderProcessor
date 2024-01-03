using ApplicationCore.Features.Orders.Shared.Domain.Entities;

namespace ApplicationCore.Features.Orders.Shared.State.DataModels;

public class AdditionalItemDataModel {

    public Guid Id { get; set; }

    public int Qty { get; set; }

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public AdditionalItem ToDomainModel() => new(Id, Qty, Description, Price);

    public static string GetQueryByOrderId() => "SELECT id, qty, description, price  FROM additional_items WHERE order_id = @OrderId;";

}
