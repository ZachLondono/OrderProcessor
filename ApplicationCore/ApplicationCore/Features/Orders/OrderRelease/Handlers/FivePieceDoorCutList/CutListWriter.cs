using ApplicationCore.Shared.Services;
using ApplicationCore.Shared.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using ExcelApp = Microsoft.Office.Interop.Excel.Application;

namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.FivePieceDoorCutList;

public class CutListWriter {

    private readonly IFileReader _fileReader;
    private readonly FivePieceDoorCutListSettings _settings;
    private readonly ILogger<CutListWriter> _logger;

    public Action<string>? OnError { get; set; }

    public CutListWriter(IFileReader fileReader, FivePieceDoorCutListSettings settings, ILogger<CutListWriter> logger) {
        _fileReader = fileReader;
        _settings = settings;
        _logger = logger;
    }

    public CutListResult? WriteCutList(CutList cutList, string outputDirectory, bool generatePDF) {

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

        var fileName = _fileReader.GetAvailableFileName(outputDirectory, $"{cutList.OrderNumber} 5-Piece DOOR CUTLIST", "xlsx");
        var fullFilePath = Path.Combine(outputDirectory, fileName);
        try {

            WriteHeader(sheet, cutList);
            WriteLineItems(sheet, cutList.Items);

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

    public void WriteHeader(Worksheet sheet, CutList cutList) {

        sheet.Range["CustomerName"].Value2 = cutList.CustomerName;
        sheet.Range["VendorName"].Value2 = cutList.VendorName;
        sheet.Range["OrderNumber"].Value2 = cutList.OrderNumber;
        sheet.Range["OrderName"].Value2 = cutList.OrderName;
        sheet.Range["OrderDate"].Value2 = cutList.OrderDate.ToShortDateString();
        sheet.Range["TotalDoorCount"].Value2 = cutList.TotalDoorCount;
        sheet.Range["Material"].Value2 = cutList.Material;
        sheet.Range["OrderNote"].Value2 = cutList.Note;

    }

    public void WriteLineItems(Worksheet sheet, IEnumerable<LineItem> lineItems) {

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

    }

    public string ExportToPDF(Workbook workbook, string outputDirectory, string orderNumber) {

        var fileName = _fileReader.GetAvailableFileName(outputDirectory, $"{orderNumber} 5-Piece DOOR CUTLIST", "pdf");
        var fullFilePath = Path.Combine(outputDirectory, fileName);
        workbook.ExportAsFixedFormat2(XlFixedFormatType.xlTypePDF, fullFilePath);

        return fullFilePath;

    }

}