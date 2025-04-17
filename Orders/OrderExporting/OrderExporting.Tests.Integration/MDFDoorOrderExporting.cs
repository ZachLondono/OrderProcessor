using Domain.ValueObjects;
using OrderExporting.DoorOrderExport;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace OrderExporting.Tests.Integration;

public class MDFDoorOrderExporting {

    [Fact]
    public void WriteDoorOrderToWorkbook() {

        // Arrange
        var order = Order;
        var item = CreateLineItem(i => {
            i.PartNumber = 5;
            i.Description = "Door";
            i.Qty = 1;
            i.Width = 2000;
            i.Height = 3000;
            i.SpecialFeatures = "Special Features";
            i.DoorType = "Door, FO";

            i.StileLeft = 1;
            i.StileRight = 2;
            i.RailTop = 3;
            i.RailBottom = 4;

            i.HingeTop = 5;
            i.HingeBottom = 6;
            i.Hinge3 = 7;
            i.Hinge4 = 8;
            i.Hinge5 = 9;
            i.Tab = 10;
            i.CupDiameter = 11;
            i.HingePattern = "Blum";
            i.CupDepth = 12;
            i.SwingDirection = "Left";

            i.HardwareReference = "Top";
            i.Hardware = "Pull - 128";
            i.HardwareTBOffset = 13;
            i.HardwareSideOffset = 14;
            i.HardwareDepth = 15;
            i.DoubleHardware = "No";

            i.Panel1 = 16;
            i.RailStile3 = 17;
            i.Panel2 = 18;
            i.RailStile4 = 19;
            i.Panel3 = 20;
            i.RailStile5 = 21;

            i.RabbetDepth = 22;
            i.RabbetWidth = 23;
            i.SquareRabbet = "Yes";
            i.PanelClearance = 24;
            i.PanelRadius = 25;
            i.PanelThickness = 26;
            i.PanelStyle = 27;

            i.MullionOpening = "Yes";
            i.MullionShape = "Square";
            i.MullionWidth = 28;
            i.HorizontalMullions = 29;
            i.VerticalMullions = 30;
            i.Row1 = 31;
            i.Col1 = 32;
            i.Row2 = 33;
            i.Col2 = 34;

            i.Ease = "No";
            i.MachinedEdges = "None";

            i.Thickness = 35;
            i.Material = "MDF";
            i.BackCut = "No";
            i.RailSeams = "No";
            i.Grain = "Vertical";
            i.PanelOrientation = "Vertical";

        });

        order.LineItems = [
            item,
            item,
            item,
            item,
            item,
            item,
            item,
            item,
            item,
            item,
        ];

        Application app = new() {
            Visible = false,
            ScreenUpdating = false,
            DisplayAlerts = false,
        };

        Workbooks workbooks = app.Workbooks;
        Workbook workbook = workbooks.Open(@"R:\Forms\_Door Order 2024, 2.112.xlsm"); // TODO: Embed test file as a resource, so it is not dependent on the file system.
        Worksheet worksheet = workbook.Sheets["MDF"];

        // Act
        try {

            var watch = new Stopwatch();
            app.Calculation = XlCalculation.xlCalculationManual;
            watch.Start();
            order.WriteToWorksheet(worksheet);
            Debug.WriteLine($"Elapsed: {watch.Elapsed}");

        } catch (Exception ex) {

            Debug.WriteLine(ex);
            throw;

        } finally {

            workbook.Close(SaveChanges:false);
            app.Quit();

            Marshal.ReleaseComObject(worksheet);
            Marshal.ReleaseComObject(workbooks);
            Marshal.ReleaseComObject(workbook);
            Marshal.ReleaseComObject(app);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();

        }

    }

    private DoorOrder Order = new() {
        OrderDate = DateTime.Now,
        DueDate = Optional<DateTime>.None,
        Company = "Customer Name",
        TrackingNumber = "Order Number",
        JobName = "Job Name",
        ProcessorOrderId = Guid.NewGuid(),
        Units = DoorOrder.METRIC_UNITS,
        VendorName = "Vendor",

        Specs = new() {
            Material = "Material",
            Style = "Framing Bead",
            EdgeProfile = "Edge Profile",
            PanelDetail = "Panel Detail",
            Finish = "Finish",
            Color = "Color",
            PanelDrop = Optional<double>.None,
            HingePattern = Optional<string>.None,
            Tab = Optional<double>.None,
            StilesRails = Optional<double>.None,
            AStyleRails = Optional<double>.None,
            AStyleDwrFrontMax = Optional<double>.None,
            DoorType = Optional<string>.None,
            HingeFromTopBottom = Optional<double>.None,
            Hardware = Optional<string>.None,
            HardwareFromTopBottom = Optional<double>.None
        },

        LineItems = []
    };

    private LineItem CreateLineItem(Action<LineItem> action) {
        var item = new LineItem() {
            PartNumber = Optional<int>.None,
            Description = "",
            Qty = 0,
            Width = 0,
            Height = 0,
            SpecialFeatures = Optional<string>.None,
            DoorType = Optional<string>.None,
            StileLeft = Optional<double>.None,
            StileRight = Optional<double>.None,
            RailTop = Optional<double>.None,
            RailBottom = Optional<double>.None,
            HingeTop = Optional<double>.None,
            HingeBottom = Optional<double>.None,
            Hinge3 = Optional<double>.None,
            Hinge4 = Optional<double>.None,
            Hinge5 = Optional<double>.None,
            Tab = Optional<double>.None,
            CupDiameter = Optional<double>.None,
            HingePattern = Optional<string>.None,
            CupDepth = Optional<double>.None,
            SwingDirection = Optional<string>.None,
            HardwareReference = Optional<string>.None,
            Hardware = Optional<string>.None,
            HardwareTBOffset = Optional<double>.None,
            HardwareSideOffset = Optional<double>.None,
            HardwareDepth = Optional<double>.None,
            DoubleHardware = Optional<string>.None,
            Panel1 = Optional<double>.None,
            RailStile3 = Optional<double>.None,
            Panel2 = Optional<double>.None,
            RailStile4 = Optional<double>.None,
            Panel3 = Optional<double>.None,
            RailStile5 = Optional<double>.None,
            RabbetDepth = Optional<double>.None,
            RabbetWidth = Optional<double>.None,
            SquareRabbet = Optional<string>.None,
            PanelClearance = Optional<double>.None,
            PanelRadius = Optional<double>.None,
            PanelThickness = Optional<double>.None,
            PanelStyle = Optional<double>.None,
            MullionOpening = Optional<string>.None,
            MullionShape = Optional<string>.None,
            MullionWidth = Optional<double>.None,
            HorizontalMullions = Optional<int>.None,
            VerticalMullions = Optional<int>.None,
            Row1 = Optional<double>.None,
            Col1 = Optional<double>.None,
            Row2 = Optional<double>.None,
            Col2 = Optional<double>.None,
            Ease = Optional<string>.None,
            MachinedEdges = Optional<string>.None,
            Thickness = Optional<double>.None,
            Material = Optional<string>.None,
            BackCut = Optional<string>.None,
            RailSeams = Optional<string>.None,
            Grain = Optional<string>.None,
            PanelOrientation = Optional<string>.None,
        };
        action(item);
        return item;
    }

}
