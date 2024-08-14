namespace ApplicationCore.Features.ClosetOrders.ClosetOrderImport;

public record OrderMessage(string SenderName, string SenderEmail, string Subject, OrderAttachment[] Attachments);
