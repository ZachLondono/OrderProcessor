namespace ApplicationCore.Features.ClosetOrders.ClosetOrderImport;

public class ClosetOrderDirectoryStructure {

    public string OrderNumber { get; init; }
    public string OrderName { get; init; }
    public string WorkingDirectory { get; init; }
    public string IncomingDirectory { get; init; }
    public string OrdersDirectory { get; init; }
    public string CutListDirectory { get; init; }

    private int _orderFileIndex = 0;

    private ClosetOrderDirectoryStructure(string orderNumber, string orderName, string workingDirectory, string incomingDirectory, string ordersDirectory, string cutListDirectory) {
        OrderNumber = orderNumber;
        OrderName = orderName;
        WorkingDirectory = workingDirectory;
        IncomingDirectory = incomingDirectory;
        OrdersDirectory = ordersDirectory;
        CutListDirectory = cutListDirectory;
    }

    public string AddFileToOrders(string filePath) {

        var fileName = Path.GetFileName(filePath);
        var orderFileName = Path.Combine(OrdersDirectory, $"{OrderNumber}{(_orderFileIndex > 0 ? $"-{_orderFileIndex}" : "")} {fileName}");

        File.Copy(filePath, orderFileName, true);
        FileUnblocker.Unblock(orderFileName);

        _orderFileIndex++;

        return orderFileName;

    }

    public static ClosetOrderDirectoryStructure BuildOrderDirectoryStructure(string directoryRoot, string number, string name) {

        var workingDirectory = Path.Combine(directoryRoot, $"{number} {name}");

        var info = Directory.CreateDirectory(workingDirectory);

        if (!info.Exists) {

            throw new InvalidOperationException("Failed to create working directory");

        }

        var incomingDir = Path.Combine(workingDirectory, "Incoming");
        var ordersDir = Path.Combine(workingDirectory, "Orders");
        var cutlistDir = Path.Combine(workingDirectory, "CUTLIST");

        var incomingInfo = Directory.CreateDirectory(incomingDir);

        if (!incomingInfo.Exists) {

            throw new InvalidOperationException("Failed to create incoming directory");

        }

        var ordersInfo = Directory.CreateDirectory(ordersDir);

        if (!ordersInfo.Exists) {

            throw new InvalidOperationException("Failed to create orders directory");

        }

        var cutlistInfo = Directory.CreateDirectory(cutlistDir);

        if (!cutlistInfo.Exists) {

            throw new InvalidOperationException("Failed to create cut list directory");

        }

        return new(number, name, workingDirectory, incomingDir, ordersDir, cutlistDir);

    }

}
