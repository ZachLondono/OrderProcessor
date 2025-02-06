using ApplicationCore.Features.HafeleMDFDoorOrders.ReadOrderFile;
using ApplicationCore.Shared.Settings;
using Domain.Services;
using Microsoft.Extensions.Options;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using Action = System.Action;
using Options = ApplicationCore.Features.HafeleMDFDoorOrders.ReadOrderFile.Options;
using Range = Microsoft.Office.Interop.Excel.Range;

namespace ApplicationCore.Features.HafeleMDFDoorOrders;

public class HafeleMDFDoorProcessor {

    private readonly ExportSettings _exportSettings;
    private readonly IFileReader _fileReader;

    public HafeleMDFDoorProcessor(IOptions<ExportSettings> exportSettings, IFileReader fileReader) {
        _exportSettings = exportSettings.Value;
        _fileReader = fileReader;
    }

    public void ProcessOrder() {

        // Create output directory
        string orderFilePath = "";
        string ordersDirectory = "";

        // Read order file
        var orderData = HafeleMDFDoorOrder.Load(orderFilePath);

        // Fill order form
        FillMDFDoorForm(orderData, ordersDirectory);

    }

    public void FillMDFDoorForm(HafeleMDFDoorOrder orderData, string outputDir) {

        List<string> errors = [];

        if (!File.Exists(_exportSettings.MDFDoorTemplateFilePath)) {
            errors.Add($"Could not find MDF order template file '{_exportSettings.MDFDoorTemplateFilePath}'");
            return;
        }

        if (!Directory.Exists(outputDir)) {
            errors.Add("Dovetail order output directory does not exist");
            return;
        }

        ExcelWrapper(_exportSettings.MDFDoorTemplateFilePath, (workbook, worksheet) => {

            FillOrderHeader(worksheet, orderData.Options);
            int offset = 1;
            foreach (var size in orderData.Sizes) {
                WriteLineItem(worksheet, offset++, size);
            }

            string fileName = _fileReader.GetAvailableFileName(outputDir, $"{orderData.Options.HafelePO} - {orderData.Options.Company} MDF DOORS", ".xlsm");
            string finalPath = Path.GetFullPath(fileName);
            workbook.SaveAs(finalPath);

        });

    }

    private static void FillOrderHeader(Worksheet sheet, Options options) {

        sheet.Range["Material"].Value2 = options.Material;
        sheet.Range["FramingBead"].Value2 = options.DoorStyle;
        sheet.Range["EdgeDetail"].Value2 = options.EdgeProfile;
        sheet.Range["PanelDetail"].Value2 = options.PanelDetail;
        if (options.Stiles != options.Rails) {
            // TODO: do something here
        }
        sheet.Range["StilesRails"].Value2 = options.Stiles;
        sheet.Range["ARail"].Value2 = options.PanelDetail;

        sheet.Range["PanelDrop"].Value2 = options.PanelDrop;
        sheet.Range["FinishOption"].Value2 = options.Finish;
        sheet.Range["FinishColor"].Value2 = ""; // TODO: get this value

        sheet.Range["HingeStyle"].Value2 = options.HingeDrilling;
        sheet.Range["Tab"].Value2 = options.HingeTab;

        sheet.Range["Vendor"].Value2 = "Hafele";

        sheet.Range["Company"].Value2 = options.Company;
        sheet.Range["JobNumber"].Value2 = options.HafelePO;
        sheet.Range["JobName"].Value2 = options.JobName;

        sheet.Range["units"].Value2 = "English (frac)";

    }

    private static void WriteLineItem(Worksheet sheet, int offset, Size size) {

        void SetOffsetRangeValue(string name, object value) => sheet.Range[name].Offset[offset].Value2 = value;

        SetOffsetRangeValue("PartNumStart", size.LineNumber);
        SetOffsetRangeValue("QtyStart", size.Qty);
        SetOffsetRangeValue("WidthStart", size.Width);
        SetOffsetRangeValue("HeightStart", size.Height);
        SetOffsetRangeValue("NoteStart", size.SpecialInstructions);

        SetOffsetRangeValue("LeftStileStart", size.LeftStile);
        SetOffsetRangeValue("RightStileStart", size.RightStile);
        SetOffsetRangeValue("TopRailStart", size.TopRail);
        SetOffsetRangeValue("BottomRailStart", size.BottomRail);

        SetOffsetRangeValue("UnitPriceStart", size.UnitPrice);

        SetOffsetRangeValue("Opening1Start", size.Panel1Height);
        SetOffsetRangeValue("Opening2Start", size.Panel2Height);
        SetOffsetRangeValue("Opening3Start", size.Panel3Height);
        SetOffsetRangeValue("Rail3Start", size.Rail3);
        SetOffsetRangeValue("Rail4Start", size.Rail4);
        SetOffsetRangeValue("Rail5Start", size.Rail5);


        // TODO: map the hafele door type to the correct value
        SetOffsetRangeValue("DescriptionStart", size.Type);
        SetOffsetRangeValue("DoorTypeStart", size.Type);

    }

    private static void ExcelWrapper(string templateFilePath, Action<Workbook, Worksheet> action) {

        List<string> errors = [];

        Application? app = null;
        Workbook? workbook = null;
        Workbooks? workbooks = null;
        Sheets? worksheets = null;
        Worksheet? worksheet = null;

        try {

            app = new() {
                DisplayAlerts = false,
                Visible = false
            };

            workbook = app.Workbooks.Open(templateFilePath);
            worksheets = workbook.Worksheets;
            worksheet = (Worksheet)worksheets["MDF"];

            action(workbook, worksheet);

        } catch (Exception ex) {

            errors.Add($"Error filling MDF door form: {ex.Message}");

        } finally {

            if (worksheet is not null) _ = Marshal.ReleaseComObject(worksheet);
            if (worksheets is not null) _ = Marshal.ReleaseComObject(worksheets);

            if (workbook is not null) {
                workbook.Close(SaveChanges: false);
                 _ = Marshal.ReleaseComObject(workbook);
            }

            if (workbooks is not null) {
                workbooks.Close();
                _ = Marshal.ReleaseComObject(workbooks);
            }

            if (app is not null) {
                app.Quit();
                _ = Marshal.ReleaseComObject(app);
            }

            // Clean up COM objects, calling these twice ensures it is fully cleaned up.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();

        }

    }

}
