using ApplicationCore.Shared.Services;
using ApplicationCore.Shared.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using ExcelApp = Microsoft.Office.Interop.Excel.Application;

namespace ApplicationCore.Features.Orders.OrderRelease.Handlers.DoweledDrawerBoxCutList;

public class DoweledDrawerBoxCutListWriter : IDoweledDrawerBoxCutListWriter {

    private readonly IFileReader _fileReader;
    private readonly DoweledDrawerBoxCutListSettings _settings;
    private readonly ILogger<DoweledDrawerBoxCutListWriter> _logger;

    public Action<string>? OnError { get; set; }

    public DoweledDrawerBoxCutListWriter(IFileReader fileReader, IOptions<DoweledDrawerBoxCutListSettings> settings, ILogger<DoweledDrawerBoxCutListWriter> logger) {
        _fileReader = fileReader;
        _settings = settings.Value;
        _logger = logger;
    }

    public DoweledDBCutListResult? WriteCutList(DoweledDrawerBoxCutList cutList, string outputDirectory, bool generatePDF) {

        if (!File.Exists(_settings.TemplateFilePath)) {
            OnError?.Invoke($"Doweled drawer box cut list template does not exist or cannot be accessed - '{_settings.TemplateFilePath}'");
            return null;

        }

        ExcelApp app;
        Workbooks workbooks;
        Workbook workbook;
        Worksheet sheet;
        try {

            app = new() {
                DisplayAlerts = false,
                Visible = false
            };

            workbooks = app.Workbooks;
            workbook = workbooks.Open(_settings.TemplateFilePath);
            sheet = workbook.Worksheets["Cut List"];

        } catch (Exception ex) {

            _logger.LogError(ex, "Exception thrown while opening doweled drawer Box cut list template");
            OnError?.Invoke("Failed to access doweled drawer Box cut list workbook template");
            return null;

        }

        var fileName = _fileReader.GetAvailableFileName(outputDirectory, $"{cutList.OrderNumber} Doweled Drawer Box CUTLIST - {cutList.Material}", "xlsx");
        var fullFilePath = Path.Combine(outputDirectory, fileName);
        try {

            WriteHeader(sheet, cutList);
            var lastRow = WriteLineItems(sheet, cutList.Items);

            sheet.PageSetup.PrintArea = $"A1:F{lastRow}";

            workbook.SaveAs(fullFilePath);

        } catch (Exception ex) {

            _logger.LogError(ex, "Exception thrown while writing doweled drawer Box cut list");
            OnError?.Invoke("Failed write doweled drawer Box cut list");
            return null;

        }

        string? pdfFilePath = null;
        if (generatePDF) {

            try {
                pdfFilePath = ExportToPDF(workbook, outputDirectory, cutList.OrderNumber);
            } catch (Exception ex) {
                _logger.LogError(ex, "Exception thrown while exporting doweled drawer Box cut list PDF");
                OnError?.Invoke("Failed to export doweled drawer Box cut list to pdf");
            }

        }

        try {

            workbook.Close();
            workbooks.Close();
            app.Quit();

            if (workbook is not null) _ = Marshal.ReleaseComObject(workbook);
            if (workbooks is not null) _ = Marshal.ReleaseComObject(workbooks);
            if (app is not null) _ = Marshal.ReleaseComObject(app);

            // Clean up COM objects, calling these twice ensures it is fully cleaned up.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();

        } catch (Exception ex) {

            _logger.LogError(ex, "Exception thrown wile releasing Excel com objects, after generating doweled drawer Box cut list");
            OnError?.Invoke("Error occurred while closing doweled drawer Box cut list");

        }

        return new(fullFilePath, pdfFilePath);

    }

    private static void WriteHeader(Worksheet sheet, DoweledDrawerBoxCutList cutList) {

        sheet.Range["CustomerName"].Value2 = cutList.CustomerName;
        sheet.Range["VendorName"].Value2 = cutList.VendorName;
        sheet.Range["OrderNumber"].Value2 = cutList.OrderNumber;
        sheet.Range["OrderName"].Value2 = cutList.OrderName;
        sheet.Range["OrderDate"].Value2 = cutList.OrderDate.ToShortDateString();
        sheet.Range["TotalBoxCount"].Value2 = cutList.TotalBoxCount;
        sheet.Range["Material"].Value2 = cutList.Material;
        sheet.Range["OrderNote"].Value2 = cutList.Note;

    }

    private static int WriteLineItems(Worksheet sheet, IEnumerable<DoweledDBCutListLineItem> lineItems) {

        int currentOffset = 1;
        foreach (var item in lineItems) {

            sheet.Range["CabNumCol"].Offset[currentOffset].Value2 = item.CabNumbers;
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

        var fileName = _fileReader.GetAvailableFileName(outputDirectory, $"{orderNumber} Doweled Drawer Box CUTLIST", "pdf");
        var fullFilePath = Path.Combine(outputDirectory, fileName);
        workbook.ExportAsFixedFormat2(XlFixedFormatType.xlTypePDF, fullFilePath);

        return fullFilePath;

    }

}
