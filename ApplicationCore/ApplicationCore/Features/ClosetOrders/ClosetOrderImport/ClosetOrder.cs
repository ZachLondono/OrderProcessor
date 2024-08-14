namespace ApplicationCore.Features.ClosetOrders.ClosetOrderImport;

public record ClosetOrder(string Number, string Name, OrderAttachment[] Attachments);