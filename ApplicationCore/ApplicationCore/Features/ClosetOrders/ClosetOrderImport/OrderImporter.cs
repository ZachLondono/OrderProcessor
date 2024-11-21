using Domain.Services.WorkingDirectory;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace ApplicationCore.Features.ClosetOrders.ClosetOrderImport;

public class OrderImporter {

    private readonly Outlook.MailItem _mailItem;

    public static readonly Dictionary<string, string> CustomerWorkingDirectoryRoots = new() {

        { "cbg57c@aol.com", @"R:\Job Scans\Closets by Glinsky - CBG" },

        { "lkerekes@tailoredcloset.com", @"R:\Job Scans\Closets-TLMid- TLMid" },
        { "tkerekes@tailoredcloset.com", @"R:\Job Scans\Closets-TLMid- TLMid" },

    };

    public OrderImporter(Outlook.MailItem mailItem) {
        _mailItem = mailItem;
    }

    public string? ImportOrderFromMailItem(ClosetOrder order, string workingDirectoryRoot) {

        if (workingDirectoryRoot is null || !Directory.Exists(workingDirectoryRoot)) {
            return null;
        }

        var structure = WorkingDirectoryStructure.Create(Path.Combine(workingDirectoryRoot, $"{order.Number} {order.Name}"), true);
        var copier = new ClosetOrderFileCopier(order.Number, structure);

        foreach (var orderAttachment in order.Attachments) {

            if (!orderAttachment.CopyToIncoming) {
                continue;
            }

            var attachment = _mailItem.Attachments[orderAttachment.Index];

            if (attachment is null) {
                continue;
            }

            var filePath = Path.Combine(structure.IncomingDirectory, attachment.FileName);
            attachment.SaveAsFile(filePath);
            if (orderAttachment.CopyToOrders) copier.AddFileToOrders(filePath);

        }

        return structure.RootDirectory;

    }

    public OrderMessage GetMessageDetails() {

        var senders = GetEmailSenders();

        var name = _mailItem.SenderName;
        var address = _mailItem.SenderEmailAddress;

        foreach (var sender in senders) {
            if (CustomerWorkingDirectoryRoots.ContainsKey(sender.EmailAddress)) {
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
            new(_mailItem.SenderName, _mailItem.SenderEmailAddress)
        ];

        var conv = _mailItem.GetConversation();

        if (conv is null) return senders.ToArray();

        var convSenders = conv.GetRootItems()
                              .OfType<Outlook.MailItem>()
                              .Select(i => new EmailSender(i.SenderName, i.SenderEmailAddress));

        senders.AddRange(convSenders);

        return senders.ToArray();

    }

    private record struct EmailSender(string Name, string EmailAddress);

}