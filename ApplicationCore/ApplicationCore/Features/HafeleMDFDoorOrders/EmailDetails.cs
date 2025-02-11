namespace ApplicationCore.Features.HafeleMDFDoorOrders;

public class EmailDetails {

    public required string Company { get; set; } = string.Empty;
    public required string OrderNumber { get; set; } = string.Empty;
    public required EmailAttachment[] Attachments { get; init; } = [];

}