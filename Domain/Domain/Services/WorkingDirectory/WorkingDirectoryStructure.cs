namespace Domain.Services.WorkingDirectory;

public record WorkingDirectoryStructure {

    public const string CUTLIST_SUB_DIRECTORY = "CUTLIST";
    public const string INCOMING_SUB_DIRECTORY = "Incoming";
    public const string ORDERS_SUB_DIRECTORY = "Orders";

    public required string RootDirectory { get; init; }
    public required string IncomingDirectory { get; init; }
    public required string CutListDirectory { get; init; }
    public required string OrdersDirectory { get; init; }

    public string CopyFileToWorkingDirectoryAsync(string filePath, bool overwrite = false) {
        var fileName = Path.GetFileName(filePath);
        return CopyFileToWorkingDirectoryAsync(filePath, fileName, overwrite);
    }

    public string CopyFileToWorkingDirectoryAsync(string filePath, string newFileName, bool overwrite = false) {
        return CopyFileToDirectory(filePath, RootDirectory, newFileName, overwrite);
    }

    public string CopyFileToIncoming(string filePath, bool overwrite = false) {
        var fileName = Path.GetFileName(filePath);
        return CopyFileToIncoming(filePath, fileName, overwrite);
    }

    public string CopyFileToIncoming(string filePath, string newFileName, bool overwrite = false) {
        return CopyFileToDirectory(filePath, IncomingDirectory, newFileName, overwrite);
    }

    public string CopyFileToCutList(string filePath, bool overwrite = false) {
        var fileName = Path.GetFileName(filePath);
        return CopyFileToCutList(filePath, fileName, overwrite);
    }

    public string CopyFileToCutList(string filePath, string newFileName, bool overwrite = false) {
        return CopyFileToDirectory(filePath, CutListDirectory, newFileName, overwrite);
    }

    public string CopyFileToOrders(string filePath, bool overwrite = false) {
        var fileName = Path.GetFileName(filePath);
        return CopyFileToOrders(filePath, fileName, overwrite);
    }

    public string CopyFileToOrders(string filePath, string newFileName, bool overwrite = false) {
        return CopyFileToDirectory(filePath, OrdersDirectory, newFileName, overwrite);
    }

    private static string CopyFileToDirectory(string filePath, string newFileName, string directory, bool overwrite) {

        var newFilePath = Path.Combine(directory, newFileName);

        if (!overwrite) {
            int counter = 1;
            var extension = Path.GetExtension(filePath);
            var fileNameWithOutExt = Path.GetFileNameWithoutExtension(filePath);
            while (File.Exists(newFilePath)) {
                newFilePath = Path.Combine(directory, $"{fileNameWithOutExt} ({counter++}){extension}");
            }
        }

        File.Copy(filePath, newFilePath, overwrite);

        return newFileName;

    }

    public Task WriteAllTextToRootAsync(string fileName, string content, bool overwrite = false)
        => WriteAllTextAsync(RootDirectory, fileName, content, overwrite);

    public Task WriteAllTextToIncomingAsync(string fileName, string content, bool overwrite = false)
        => WriteAllTextAsync(IncomingDirectory, fileName, content, overwrite);

    public Task WriteAllTextToCutListAsync(string fileName, string content, bool overwrite = false)
        => WriteAllTextAsync(CutListDirectory, fileName, content, overwrite);

    public Task WriteAllTextToOrdersAsync(string fileName, string content, bool overwrite = false)
        => WriteAllTextAsync(OrdersDirectory, fileName, content, overwrite);

    private async Task WriteAllTextAsync(string directory, string fileName, string content, bool overwrite = false) {

        var path = Path.Combine(RootDirectory, fileName);

        if (!overwrite) {
            int counter = 1;
            var extension = Path.GetExtension(path);
            var fileNameWithOutExt = Path.GetFileNameWithoutExtension(path);
            while (File.Exists(path)) {
                path = Path.Combine(directory, $"{fileNameWithOutExt} ({counter++}){extension}");
            }
        }

        await File.WriteAllTextAsync(path, content);


    }

    public static WorkingDirectoryStructure Create(string workingDirectory, bool createDirectories = false) {

        if (createDirectories && !File.Exists(workingDirectory)) {
            Directory.CreateDirectory(workingDirectory);
        }

        return new() {
            RootDirectory = workingDirectory,
            IncomingDirectory = GetIncomingDirectory(workingDirectory, createDirectories),
            CutListDirectory = GetCutListDirectory(workingDirectory, createDirectories),
            OrdersDirectory = GetOrdersDirectory(workingDirectory, createDirectories)
        };

    }

    private static string GetCutListDirectory(string workingDirectory, bool createIfNotExists = false) {

        var directory = Path.Combine(workingDirectory, CUTLIST_SUB_DIRECTORY);

        if (createIfNotExists && !File.Exists(directory)) {
            _ = Directory.CreateDirectory(directory);
        }

        return directory;

    }

    private static string GetIncomingDirectory(string workingDirectory, bool createIfNotExists = false) {

        var directory = Path.Combine(workingDirectory, INCOMING_SUB_DIRECTORY);

        if (createIfNotExists && !File.Exists(directory)) {
            _ = Directory.CreateDirectory(directory);
        }

        return directory;

    }

    private static string GetOrdersDirectory(string workingDirectory, bool createIfNotExists = false) {

        var directory = Path.Combine(workingDirectory, ORDERS_SUB_DIRECTORY);

        if (createIfNotExists && !File.Exists(directory)) {
            _ = Directory.CreateDirectory(directory);
        }

        return directory;

    }

}
