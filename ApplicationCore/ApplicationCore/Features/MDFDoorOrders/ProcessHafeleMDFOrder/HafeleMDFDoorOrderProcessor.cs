using Domain.Services;
using Microsoft.Office.Interop.Excel;
using OrderExporting.DoorOrderExport;
using OrderLoading.LoadHafeleMDFDoorSpreadsheetOrderData.ReadOrderFile;
using System.Runtime.InteropServices;

namespace ApplicationCore.Features.MDFDoorOrders.ProcessHafeleMDFOrder;

public class HafeleMDFDoorOrderProcessor {

    private readonly IFileReader _fileReader;

    public HafeleMDFDoorOrderProcessor(IFileReader fileReader) {
        _fileReader = fileReader;
    }

    public async Task ProcessOrderAsync(ProcessOptions options) {

        var orderData = await Task.Run(() => LoadOrderData(options.DataFile));

        if (orderData is null) {

            // Could not load order data
            return;

        }

        if (options.GenerateInvoice) {
            var invoice = GenerateInvoice();
            if (options.SendInvoiceEmail) {
                SendInvoiceEmail();
            }
        }

        if (options.FillOrderSheet) {
            _ = await Task.Run(() => FillOrderSheet(orderData, options.HafelePO, options.OrderSheetTemplatePath, options.OrderSheetOutputDirectory));
        }

        if (options.PostToGoogleSheets) {
            await PostOrderToGoogleSheet(orderData, options.HafelePO);
        }

    }

    private HafeleMDFDoorOrder? LoadOrderData(string orderData) {
        var result = HafeleMDFDoorOrder.Load(orderData);
        //result.Warnings
        //result.Errors
        return result.Data;
    }

    private string GenerateInvoice() => throw new NotImplementedException();

    private string SendInvoiceEmail() => throw new NotImplementedException();

    private IEnumerable<string> FillOrderSheet(HafeleMDFDoorOrder order, string hafelePO, string template, string outputDirectory) {

        var orderFiles = CreateDoorOrders(order, hafelePO);

        Application app = new() {
            DisplayAlerts = false,
            Visible = false,
            ScreenUpdating = false
        };

        List<string> filesGenerated = new();

        var workbooks = app.Workbooks;
        bool wasExceptionThrown = false;
        foreach (var orderFile in orderFiles) {

            Workbook? workbook = null;

            try {

                workbook = workbooks.Open(template, ReadOnly: true);

                app.Calculation = XlCalculation.xlCalculationManual;

                var worksheets = workbook.Worksheets;
                Worksheet worksheet = (Worksheet)worksheets["MDF"];

                orderFile.WriteToWorksheet(worksheet);

                _ = Marshal.ReleaseComObject(worksheet);
                _ = Marshal.ReleaseComObject(worksheets);

                string fileName = _fileReader.GetAvailableFileName(outputDirectory, $"{hafelePO} - {order.Options.JobName} MDF DOORS", ".xlsm");
                string finalPath = Path.GetFullPath(fileName);

                workbook.SaveAs(finalPath);

                filesGenerated.Add(finalPath);

            } catch (Exception ex) {

                //_logger.LogError(ex, "Exception thrown while filling door order group");
                wasExceptionThrown = true;

            } finally {

                if (workbook is not null) {
                    workbook.Close(SaveChanges: false);
                    _ = Marshal.ReleaseComObject(workbook);
                }

            }

        }

        workbooks.Close();
        app?.Quit();

        _ = Marshal.ReleaseComObject(workbooks);
        if (app is not null) _ = Marshal.ReleaseComObject(app);

        // Clean up COM objects, calling these twice ensures it is fully cleaned up.
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        GC.WaitForPendingFinalizers();

        //string? error = wasExceptionThrown ? "An error occurred while trying to write one or more door orders" : null;
        //return new(filesGenerated, error);

        return filesGenerated;

    }

    private static IEnumerable<DoorOrder> CreateDoorOrders(HafeleMDFDoorOrder order, string hafelePO) {

        var doors = order.GetProducts();

        var groups = GeneralSpecs.SeparatedDoorsBySpecs(doors);

        for (int i = 0; i < groups.Length; i++) {

            // TODO: Optimization - find the most common frame width and set that to the default for the group. Then do not overwrite those values in the line items.

            var group = groups[i];

            yield return new() {
                OrderDate = order.Options.Date,
                DueDate = order.Options.GetDueDate(),
                Company = order.Options.Company,
                TrackingNumber = $"{hafelePO}{(groups.Length == 1 ? string.Empty : $"-{i + 1}")}",
                JobName = order.Options.JobName,
                ProcessorOrderId = Guid.Empty,
                Units = DoorOrder.METRIC_UNITS,
                VendorName = "Hafele America Co.",
                Specs = group.Key,
                LineItems = group.Select(LineItem.FromDoor)
            };

        }
    }

    private static async Task PostOrderToGoogleSheet(HafeleMDFDoorOrder order, string hafelePO) {

        var row = new GoogleSheetRow() {
            FileDate = order.Options.Date.ToShortDateString(),
            HafelePO = hafelePO,
            ProjectNumber = order.Options.HafeleOrderNumber,
            ConfigNumber = "",
            CustomerName = order.Options.Company,
            CustomerPO = order.Options.JobName,
            TotalItemCount = order.Sizes.Sum(s => s.Qty).ToString(),
            ShipDate = order.Options.GetDueDate().ToShortDateString(),
            InvoiceAmount = order.GetInvoiceAmount().ToString(),
            TrackingNumber = ""
        };

        await row.PostData();

    }

}
