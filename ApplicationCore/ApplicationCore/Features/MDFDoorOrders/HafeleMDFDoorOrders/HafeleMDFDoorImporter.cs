using ApplicationCore.Features.MDFDoorOrders.HafeleMDFDoorOrders.ReadOrderFile;
using ApplicationCore.Shared.Settings;
using Domain.Services;
using Microsoft.Extensions.Options;
using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Outlook;
using System.Runtime.InteropServices;
using Exception = System.Exception;
using Options = ApplicationCore.Features.MDFDoorOrders.HafeleMDFDoorOrders.ReadOrderFile.Options;
using Application = Microsoft.Office.Interop.Excel.Application;

namespace ApplicationCore.Features.MDFDoorOrders.HafeleMDFDoorOrders;

public class HafeleMDFDoorImporter {

    private const string _workingDirectoryRoot = @"R:\Door Orders\Hafele\Orders";
    private readonly ExportSettings _exportSettings;
    private readonly IFileReader _fileReader;

    public HafeleMDFDoorImporter(IOptions<ExportSettings> exportSettings, IFileReader fileReader) {
        _exportSettings = exportSettings.Value;
        _fileReader = fileReader;
    }

    public void ImportOrderFromMailItem(EmailDetails details, MailItem mailItem) {

        var structure = CreateDirectoryStructure(_workingDirectoryRoot, $"{details.Company} - {details.OrderNumber}");

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
            FillMDFDoorForm(orderData, structure.OrdersDirectory, attachment.FileName, orderFilePath);

        }

    }

    public static DirectoryStructure CreateDirectoryStructure(string workingDirectoryRoot, string workingDirectoryName) {

        string workingDirectory = Path.Combine(_workingDirectoryRoot, workingDirectoryName);
        var dirInfo = Directory.CreateDirectory(workingDirectory);
        if (!dirInfo.Exists) {
            throw new InvalidOperationException($"Failed to create directory '{workingDirectory}'");
        }

        string cutlistDir = Path.Combine(workingDirectory, "CUTLIST");
        _ = Directory.CreateDirectory(cutlistDir);
        string incomingDir = Path.Combine(workingDirectory, "incoming");
        _ = Directory.CreateDirectory(incomingDir);
        string ordersDir = Path.Combine(workingDirectory, "orders");
        _ = Directory.CreateDirectory(ordersDir);

        return new DirectoryStructure(workingDirectory, incomingDir, ordersDir, cutlistDir );

    }

    public void FillMDFDoorForm(HafeleMDFDoorOrder orderData, string outputDir, string fileName, string incomingDataFile) {

        List<string> errors = [];

        if (!File.Exists(_exportSettings.MDFDoorTemplateFilePath)) {
            errors.Add($"Could not find MDF order template file '{_exportSettings.MDFDoorTemplateFilePath}'");
            return;
        }

        if (!Directory.Exists(outputDir)) {
            errors.Add("MDF order output directory does not exist");
            return;
        }

        ExcelWrapper(_exportSettings.MDFDoorTemplateFilePath, (workbook, worksheet) => {

            FillOrderHeader(worksheet, orderData.Options, incomingDataFile);
            int offset = 1;
            foreach (var size in orderData.Sizes) {
                WriteLineItem(worksheet, offset++, size);
            }

            string finalPath = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(fileName) + ".xlsm");
            workbook.SaveAs(finalPath);

        });

    }

    private static void FillOrderHeader(Worksheet sheet, Options options, string incomingDataFile) {

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
        sheet.Range["FinishOption"].Value2 = options.Finish; // TODO: map these values
        sheet.Range["FinishColor"].Value2 = ""; // TODO: get this value

        sheet.Range["HingeStyle"].Value2 = options.HingeDrilling;
        sheet.Range["HingeTab"].Value2 = options.HingeTab;

        sheet.Range["Vendor"].Value2 = "Hafele America Co.";

        sheet.Range["Company"].Value2 = options.Company;
        sheet.Range["JobNumber"].Value2 = options.HafelePO;
        sheet.Range["JobName"].Value2 = options.JobName;

        sheet.Range["units"].Value2 = "English (frac)";

        sheet.Range["Hafele_Incoming_Data"].Value2 = incomingDataFile;

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