using Domain.Services.WorkingDirectory;

namespace ApplicationCore.Features.ClosetOrders.ClosetOrderImport;

public class ClosetOrderFileCopier {

    private readonly string _orderNumber;
    private readonly WorkingDirectoryStructure _workingDirectoryStructure;
    private int _orderFileIndex = 0;

    public ClosetOrderFileCopier(string orderNumber, WorkingDirectoryStructure workingDirectoryStructure) {
        _orderNumber = orderNumber;
        _workingDirectoryStructure = workingDirectoryStructure; 
    }

    public string AddFileToOrders(string filePath) {

        var fileName = Path.GetFileName(filePath);
        var newFileName = $"{_orderNumber}{(_orderFileIndex > 0 ? $"-{_orderFileIndex}" : "")} {fileName}");

        var newPath = _workingDirectoryStructure.CopyFileToOrders(filePath, newFileName, false);

        FileUnblocker.Unblock(newPath);

        _orderFileIndex++;

        return newPath;

    }

}
