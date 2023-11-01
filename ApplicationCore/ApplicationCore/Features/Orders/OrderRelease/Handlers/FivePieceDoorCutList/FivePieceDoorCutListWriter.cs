using ApplicationCore.Shared.Services;
using ApplicationCore.Shared.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using ExcelApp = Microsoft.Office.Interop.Excel.Application;

namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.FivePieceDoorCutList;

public class FivePieceDoorCutListWriter : IFivePieceDoorCutListWriter {

    private readonly IFileReader _fileReader;
    private readonly FivePieceDoorCutListSettings _settings;
    private readonly ILogger<FivePieceDoorCutListWriter> _logger;

    public Action<string>? OnError { get; set; }

    public FivePieceDoorCutListWriter(IFileReader fileReader, IOptions<FivePieceDoorCutListSettings> settings, ILogger<FivePieceDoorCutListWriter> logger) {
        _fileReader = fileReader;
        _settings = settings.Value;
        _logger = logger;
    }

    public CutListResult? WriteCutList(FivePieceCutList cutList, string outputDirectory, bool generatePDF) {

        if (!File.Exists(_settings.TemplateFilePath)) {
            OnError?.Invoke($"5-Pieced door cut list template does not exist or cannot be accessed - '{_settings.TemplateFilePath}'");
            return null;
        }
        ExcelApp app;
        Workbook workbook;
        Worksheet sheet;
        try {

            app = new() {
                DisplayAlerts = false,
                Visible = false
            };

            workbook = app.Workbooks.Open(_settings.TemplateFilePath);
            sheet = workbook.Worksheets["Cut List"];

        } catch (Exception ex) {

            _logger.LogError(ex, "Exception thrown while opening 5-piece door cut list template");
            OnError?.Invoke("Failed to access 5-piece door cut list workbook template");
            return null;

        }

        var fileName = _fileReader.GetAvailableFileName(outputDirectory, $"{cutList.OrderNumber} 5-Piece DOOR CUTLIST - {cutList.Material}", "xlsx");
        var fullFilePath = Path.Combine(outputDirectory, fileName);
        try {

            WriteHeader(sheet, cutList);
            var lastRow = WriteLineItems(sheet, cutList.Items);

            sheet.PageSetup.PrintArea = $"B1:G{lastRow}";

            workbook.SaveAs(fullFilePath);

        } catch (Exception ex) {

            _logger.LogError(ex, "Exception thrown while writing 5-piece door cut list");
            OnError?.Invoke("Failed write 5-piece door cut list");
            return null;

        }

        string? pdfFilePath = null;
        if (generatePDF) {

            try {
                pdfFilePath = ExportToPDF(workbook, outputDirectory, cutList.OrderNumber);
            } catch (Exception ex) {
                _logger.LogError(ex, "Exception thrown while exporting 5-piece door cut list PDF");
                OnError?.Invoke("Failed to export 5-piece door cut list to pdf");
            }

        }

        try {

            workbook.Close();
            app.Quit();

            if (workbook is not null) _ = Marshal.ReleaseComObject(workbook);
            if (app is not null) _ = Marshal.ReleaseComObject(app);

            // Clean up COM objects, calling these twice ensures it is fully cleaned up.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();

        } catch (Exception ex) {

            _logger.LogError(ex, "Exception thrown wile releasing Excel com objects, after generating 5-piece door cut list");
            OnError?.Invoke("Error occurred while closing 5-piece door cut list");

        }

        return new(fullFilePath, pdfFilePath);

    }

    private static void WriteHeader(Worksheet sheet, FivePieceCutList cutList) {

        sheet.Range["CustomerName"].Value2 = cutList.CustomerName;
        sheet.Range["VendorName"].Value2 = cutList.VendorName;
        sheet.Range["OrderNumber"].Value2 = cutList.OrderNumber;
        sheet.Range["OrderName"].Value2 = cutList.OrderName;
        sheet.Range["OrderDate"].Value2 = cutList.OrderDate.ToShortDateString();
        sheet.Range["TotalDoorCount"].Value2 = cutList.TotalDoorCount;
        sheet.Range["Material"].Value2 = cutList.Material;
        sheet.Range["OrderNote"].Value2 = cutList.Note;

    }

    private static int WriteLineItems(Worksheet sheet, IEnumerable<LineItem> lineItems) {

        int currentOffset = 1;
        foreach (var item in lineItems) {

            sheet.Range["CabNumCol"].Offset[currentOffset].Value2 = item.CabNumber;
            sheet.Range["PartNameCol"].Offset[currentOffset].Value2 = item.PartName;
            sheet.Range["QtyCol"].Offset[currentOffset].Value2 = item.Qty;
            sheet.Range["WidthCol"].Offset[currentOffset].Value2 = item.Width;
            sheet.Range["LengthCol"].Offset[currentOffset].Value2 = item.Length;
            sheet.Range["NoteCol"].Offset[currentOffset].Value2 = item.Note;

            currentOffset++;

        }

        return sheet.Range["CabNumCol"].Offset[currentOffset].Row;

    }

    private string ExportToPDF(Workbook workbook, string outputDirectory, string orderNumber) {

        var fileName = _fileReader.GetAvailableFileName(outputDirectory, $"{orderNumber} 5-Piece DOOR CUTLIST", "pdf");
        var fullFilePath = Path.Combine(outputDirectory, fileName);
        workbook.ExportAsFixedFormat2(XlFixedFormatType.xlTypePDF, fullFilePath);

        return fullFilePath;

    }

}