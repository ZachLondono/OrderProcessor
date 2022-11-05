namespace ApplicationCore.Features.Orders.Loader.Providers.DTO;

public record AdditionalItemData {

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

}