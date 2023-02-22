using ApplicationCore.Features.Orders.Shared.Domain.Entities;

namespace ApplicationCore.Features.Orders.Shared.State.DataModels;

public class AdditionalItemDataModel {

    public Guid Id { get; set; }

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public AdditionalItem AsDomainModel() => new AdditionalItem(Id, Description, Price);

    public static string GetQueryByOrderId() => "SELECT id, description, price FROM additionalitems WHERE orderid = @OrderId;";

}
