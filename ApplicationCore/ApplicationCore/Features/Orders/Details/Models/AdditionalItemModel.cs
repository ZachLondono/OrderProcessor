namespace ApplicationCore.Features.Orders.Details.Models;

public class AdditionalItemModel {

    public required Guid Id { get; set; }
    public required string Description { get; set; }
    public required decimal Price { get; set; }
    public required bool IsService { get; set; }

}
