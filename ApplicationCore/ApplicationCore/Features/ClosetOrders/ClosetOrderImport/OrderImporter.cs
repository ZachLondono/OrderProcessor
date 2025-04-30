using Domain.Extensions;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace ApplicationCore.Features.ClosetOrders.ClosetOrderImport;

public class OrderImporter {

    private readonly Outlook.MailItem _mailItem;

    public static readonly Dealer[] Dealers = [
        new( [ "cbg57c@aol.com", "closetman1@gmail.com" ], @"R:\Job Scans\Closets by Glinsky - CBG", "CBG"),
        new( [ "tkerekes@tailoredcloset.com", "lkerekes@tailoredcloset.com" ], @"R:\Job Scans\Closets-TLMid- TLMid", "TLMid"),
    ];

    public OrderImporter(Outlook.MailItem mailItem) {
        _mailItem = mailItem;
    }

    public string? ImportOrderFromMailItem(ClosetOrder order, string workingDirectoryRoot, bool writeOrderNumber) {

        if (workingDirectoryRoot is null || !Directory.Exists(workingDirectoryRoot)) {
            return null;
        }

        var structure = ClosetOrderDirectoryStructure.BuildOrderDirectoryStructure(workingDirectoryRoot, order.Number, order.Name);

        if (order.Attachments.Length == 0) {
            return structure.WorkingDirectory;
        }

        foreach (var orderAttachment in order.Attachments) {

            if (!orderAttachment.CopyToIncoming) {
                continue;
            }

            var attachment = _mailItem.Attachments[orderAttachment.Index];

            if (attachment is null) {
                continue;
            }

            ImportOrderFile(structure, attachment, orderAttachment.CopyToOrders, writeOrderNumber);

        }

        return structure.WorkingDirectory;

    }

    private static void ImportOrderFile(ClosetOrderDirectoryStructure structure, Outlook.Attachment attachment, bool copyToOrders, bool writeOrderNumber) {

        var savePath = Path.Combine(structure.IncomingDirectory, attachment.FileName);
        attachment.SaveAsFile(savePath);

        if (copyToOrders) _ = structure.AddFileToOrders(savePath, writeOrderNumber);

    }

    public OrderMessage GetMessageDetails() {

        var senders = GetEmailSenders();

        var name = _mailItem.SenderName;
        var address = GetEmailAddress(_mailItem);

        foreach (var sender in senders) {
            if (Dealers.Any(d => d.IncomingEmails.Any(e => e.Equals(sender.EmailAddress)))) {
                name = sender.Name;
                address = sender.EmailAddress;
                break;
            }
        }

        var subject = _mailItem.Subject;

        List<OrderAttachment> attachments = [];
        foreach (Outlook.Attachment attachment in _mailItem.Attachments) {
            var ext = Path.GetExtension(attachment.FileName);
            bool incoming = true;
            bool orders = ext.Equals(".xlsm");
            attachments.Add(new(attachment.Index, attachment.FileName, incoming, orders));
        }

        return new(name, address, subject, attachments.ToArray());

    }

    private EmailSender[] GetEmailSenders() {

        List<EmailSender> senders = [
            new(_mailItem.SenderName, GetEmailAddress(_mailItem))
        ];

        var conv = _mailItem.GetConversation();

        if (conv is null) return senders.ToArray();

        conv.GetRootItems()
            .OfType<Outlook.MailItem>()
            .Select(i => new EmailSender(i.SenderName, GetEmailAddress(i)))
            .ForEach(s => senders.Add(s));

        foreach (var item in conv.GetRootItems()) {
            senders.AddRange(GetAllSendersFromConversation(item, conv));
        }

        return senders.ToArray();

    }

    private static string GetEmailAddress(Outlook.MailItem mailItem) {

        if (mailItem.SenderEmailType == "EX") {
            return mailItem.Sender.GetExchangeUser().PrimarySmtpAddress;
        }

        return mailItem.SenderEmailAddress;

    }

    private static List<EmailSender> GetAllSendersFromConversation(object item, Outlook.Conversation conversation) {
        List<EmailSender> senders = [];
        GetAllSendersFromConversationHelper(item, conversation, senders);
        return senders;
    }

    private static void GetAllSendersFromConversationHelper(object item, Outlook.Conversation conversation, List<EmailSender> senders) {

        Outlook.SimpleItems items = conversation.GetChildren(item);

        if (items.Count <= 0) {
            return;
        }

        foreach (object childItem in items) {

            if (childItem is Outlook.MailItem mailItem) {
                senders.Add(new(mailItem.SenderName, GetEmailAddress(mailItem)));
            }

            GetAllSendersFromConversationHelper(childItem, conversation, senders);

        }

    }

    private record struct EmailSender(string Name, string EmailAddress);

    public record Dealer(string[] IncomingEmails, string OutputDirectory, string OrderNumberPrefix);

}