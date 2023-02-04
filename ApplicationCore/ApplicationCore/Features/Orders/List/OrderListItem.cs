namespace ApplicationCore.Features.List;

public class OrderListItem {

    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Number { get; init; } = string.Empty;

    public DateTime OrderDate { get; init; }

    public string CustomerName { get; init; } = string.Empty;

    public string VendorName { get; set; } = string.Empty;

    public Guid VendorId { get; init; }

    public int ItemCount { get; init; }

}
