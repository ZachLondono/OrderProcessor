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

    public bool ImportOrderFromMailItem(ClosetOrder order) {

        if (!CustomerWorkingDirectoryRoots.TryGetValue(_mailItem.SenderEmailAddress, out string? workingDirectoryRoot)) {
            return false;
        }

        if (workingDirectoryRoot is null || !Directory.Exists(workingDirectoryRoot)) {
            return false;
        }

        var structure = ClosetOrderDirectoryStructure.BuildOrderDirectoryStructure(workingDirectoryRoot, order.Number, order.Name);

        if (order.Attachments.Length == 0) {
            return true;
        }

        bool wereAttachmentsCopies = true;

        foreach (var orderAttachment in order.Attachments) {

            if (!orderAttachment.CopyToIncoming) {
                continue;
            }

            var attachment = _mailItem.Attachments[orderAttachment.Index];

            if (attachment is null) {
                wereAttachmentsCopies = false;
                continue;
            }

            ImportOrderFile(structure, attachment, orderAttachment.CopyToOrders);

        }

        return wereAttachmentsCopies;

    }

    private static void ImportOrderFile(ClosetOrderDirectoryStructure structure, Outlook.Attachment attachment, bool copyToOrders) {

        var savePath = Path.Combine(structure.IncomingDirectory, attachment.FileName);
        attachment.SaveAsFile(savePath);

        if (copyToOrders) _ = structure.AddFileToOrders(savePath);

    }

    public OrderMessage GetMessageDetails() {

        var name = _mailItem.SenderName;
        var address = _mailItem.SenderEmailAddress;
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

}