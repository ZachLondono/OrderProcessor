using Domain.Services;

namespace OrderLoading.LoadHafeleMDFDoorSpreadsheetOrderData;

public class HafeleMDFDoorOrderProvider : IOrderProvider {

    private const string _workingDirectoryRoot = @"R:\Door Orders\Hafele\Orders";
    private readonly IFileReader _fileReader;

    public IOrderLoadWidgetViewModel? OrderLoadingViewModel { get; set; }

    public HafeleMDFDoorOrderProvider(IFileReader fileReader) {
        _fileReader = fileReader;
    }

    public Task<OrderData?> LoadOrderData(string source) {

        throw new NotImplementedException();

    }

    /*
    public void ImportOrderFromMailItem(EmailDetails details, MailItem mailItem) {

        var structure = CreateDirectoryStructure(_workingDirectoryRoot, $"{details.OrderNumber} - {details.Company}");

        if (structure is null) {
            return;
        }

        foreach (var emailAttachment in details.Attachments) {

            if (!emailAttachment.CopyToIncoming) {
                continue;
            }

            var attachment = mailItem.Attachments[emailAttachment.Index];

            if (attachment is null) {
                continue;
            }

            var orderFilePath = Path.Combine(structure.IncomingDirectory, attachment.FileName);
            attachment.SaveAsFile(orderFilePath);

            if (!emailAttachment.IsOrderForm) {
                continue;
            }

            var orderData = HafeleMDFDoorOrder.Load(orderFilePath);

            foreach (var error in orderData.Errors) {
                OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, error);
            }

            foreach (var warning in orderData.Warnings) {
                OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Warning, warning);
            }

        }

    }
    */

    public DirectoryStructure? CreateDirectoryStructure(string workingDirectoryRoot, string workingDirectoryName) {

        string workingDirectory = Path.Combine(_workingDirectoryRoot, workingDirectoryName);
        var dirInfo = Directory.CreateDirectory(workingDirectory);
        if (!dirInfo.Exists) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"Failed to create directory '{workingDirectory}'");
            return null;
        }

        string cutlistDir = Path.Combine(workingDirectory, "CUTLIST");
        _ = Directory.CreateDirectory(cutlistDir);
        string incomingDir = Path.Combine(workingDirectory, "incoming");
        _ = Directory.CreateDirectory(incomingDir);
        string ordersDir = Path.Combine(workingDirectory, "orders");
        _ = Directory.CreateDirectory(ordersDir);

        return new DirectoryStructure(workingDirectory, incomingDir, ordersDir, cutlistDir );

    }

}