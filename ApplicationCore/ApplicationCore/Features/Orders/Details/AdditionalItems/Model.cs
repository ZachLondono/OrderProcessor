namespace ApplicationCore.Features.Orders.Details.AdditionalItems;

public class Model {

    public required Guid Id { get; set; }
    public required string Description { get; set; }
    public required decimal Price { get; set; }
    public required bool IsService { get; set; }

}
