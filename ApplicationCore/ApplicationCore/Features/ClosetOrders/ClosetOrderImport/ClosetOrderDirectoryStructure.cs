using Microsoft.Office.Interop.Excel;
using System.Diagnostics;
using System.Runtime.InteropServices;

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

    public string AddFileToOrders(string filePath, bool writeOrderNumber) {

        var fileName = Path.GetFileName(filePath);
        var actualOrderNumber = $"{OrderNumber}{(_orderFileIndex > 0 ? $"-{_orderFileIndex}" : "")}";
        var orderFileName = Path.Combine(OrdersDirectory, $"{actualOrderNumber} {fileName}");

        if (writeOrderNumber) {
            TryToFillOrderForm(filePath, orderFileName, actualOrderNumber);
        } else {
            File.Copy(filePath, orderFileName, true);
            FileUnblocker.Unblock(orderFileName);
        }

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

    public static void TryToFillOrderForm(string originalFilePath, string newFilePath, string number) {

        Application? app = null;
        Workbooks? workbooks = null;
        Workbook? workbook = null;
        Sheets? sheets = null;
        Worksheet? sheet = null;

        try {

            app = new Application();
            app.DisplayAlerts = false;

            workbooks = app.Workbooks;
            workbook = workbooks.Open(originalFilePath, ReadOnly: false);
            sheets = workbook.Worksheets;
            sheet = sheets["Cover"];

            sheet.Range["OrderNum"].Value2 = number;

            workbook.SaveAs2(newFilePath);
            workbook.Close(SaveChanges: true);

        } catch (Exception ex) {

            Debug.WriteLine(ex.Message);

        } finally {

            if (sheet is not null) Marshal.ReleaseComObject(sheet);
            if (sheets is not null) Marshal.ReleaseComObject(sheets);
            if (workbook is not null) Marshal.ReleaseComObject(workbook);

            if (workbooks is not null) {
                workbooks.Close();
                Marshal.ReleaseComObject(workbooks);
            }

            if (app is not null) {
                app.Quit();
                Marshal.ReleaseComObject(app);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();

        }

    }

}
