using Domain.ValueObjects;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using Range = Microsoft.Office.Interop.Excel.Range;

namespace OrderExporting.DoorOrderExport;

public class LineItemWriter : IDisposable {

    private bool _isInitialized = false;
    private Range? _partNumRef;
    private Range? _descriptionRef;
    private Range? _qtyRef;
    private Range? _widthRef;
    private Range? _heightRef;
    private Range? _noteRef;
    private Range? _typeRef;
    private Range? _leftStileRef;
    private Range? _rightStileRef;
    private Range? _topRailRef;
    private Range? _bottomRailRef;
    private Range? _hingeTopRef;
    private Range? _hingeBottomRef;
    private Range? _hinge3Ref;
    private Range? _hinge4Ref;
    private Range? _hinge5Ref;
    private Range? _tabRef;
    private Range? _cupDiaRef;
    private Range? _hingePatternRef;
    private Range? _cupDepthRef;
    private Range? _swingDirRef;
    private Range? _hardwareRefRef;
    private Range? _hardwareRef;
    private Range? _hardwareTBOffsetRef;
    private Range? _hardwareSideOffsetRef;
    private Range? _hardwareDepthRef;
    private Range? _doubleHardwareRef;
    private Range? _panel1Ref;
    private Range? _rail3Ref;
    private Range? _opening2Ref;
    private Range? _rail4Ref;
    private Range? _opening3Ref;
    private Range? _rail5Ref;
    private Range? _rabbetDepth;
    private Range? _rabbetWidth;
    private Range? _squareRabbet;
    private Range? _panelClearance;
    private Range? _panelRadius;
    private Range? _panelThickness;
    private Range? _panelStyle;
    private Range? _mullionOpening;
    private Range? _mullionShape;
    private Range? _mullionWidth;
    private Range? _horizMullions;
    private Range? _vertMullions;
    private Range? _row1Ref;
    private Range? _col1Ref;
    private Range? _row2Ref;
    private Range? _col2Ref;
    private Range? _ease;
    private Range? _machinedEdge;
    private Range? _thickness;
    private Range? _material;
    private Range? _backCut;
    private Range? _railSeams;
    private Range? _grain;
    private Range? _panelOrientation;

    private readonly Worksheet _worksheet;
    private int _offset = 1;

    public LineItemWriter(Worksheet worksheet) {
        _worksheet = worksheet;
    }

    private void Initialize() {

        if (_isInitialized) return;

        _partNumRef = _worksheet.Range["PartNumStart"];
        _descriptionRef = _worksheet.Range["DescriptionStart"];
        _qtyRef = _worksheet.Range["QtyStart"];
        _widthRef = _worksheet.Range["WidthStart"];
        _heightRef = _worksheet.Range["HeightStart"];
        _noteRef = _worksheet.Range["NoteStart"];
        _typeRef = _worksheet.Range["DoorTypeStart"];
        _leftStileRef = _worksheet.Range["LeftStileStart"];
        _rightStileRef = _worksheet.Range["RightStileStart"];
        _topRailRef = _worksheet.Range["TopRailStart"];
        _bottomRailRef = _worksheet.Range["BottomRailStart"];
        _hingeTopRef = _worksheet.Range["T15"];
        _hingeBottomRef = _worksheet.Range["U15"];
        _hinge3Ref = _worksheet.Range["V15"];
        _hinge4Ref = _worksheet.Range["W15"];
        _hinge5Ref = _worksheet.Range["X15"];
        _tabRef = _worksheet.Range["Y15"];
        _cupDiaRef = _worksheet.Range["Z15"];
        _hingePatternRef = _worksheet.Range["AA15"];
        _cupDepthRef = _worksheet.Range["AB15"];
        _swingDirRef = _worksheet.Range["AC15"];
        _hardwareRefRef = _worksheet.Range["AD15"];
        _hardwareRef = _worksheet.Range["AE15"];
        _hardwareTBOffsetRef = _worksheet.Range["AF15"];
        _hardwareSideOffsetRef = _worksheet.Range["AG15"];
        _hardwareDepthRef = _worksheet.Range["AH15"];
        _doubleHardwareRef = _worksheet.Range["AI15"];
        _panel1Ref = _worksheet.Range["Opening1Start"];
        _rail3Ref = _worksheet.Range["Rail3Start"];
        _opening2Ref = _worksheet.Range["Opening2Start"];
        _rail4Ref = _worksheet.Range["Rail4Start"];
        _opening3Ref = _worksheet.Range["Opening3Start"];
        _rail5Ref = _worksheet.Range["Rail5Start"];
        _rabbetDepth = _worksheet.Range["AP15"];
        _rabbetWidth = _worksheet.Range["RabbetWidthStart"];
        _squareRabbet = _worksheet.Range["AR15"];
        _panelClearance = _worksheet.Range["AS15"];
        _panelRadius = _worksheet.Range["AT15"];
        _panelThickness = _worksheet.Range["AU15"];
        _panelStyle = _worksheet.Range["AV15"];
        _mullionOpening = _worksheet.Range["AW15"];
        _mullionShape = _worksheet.Range["AX15"];
        _mullionWidth = _worksheet.Range["AY15"];
        _horizMullions = _worksheet.Range["AZ15"];
        _vertMullions = _worksheet.Range["BA15"];
        _row1Ref = _worksheet.Range["BB15"];
        _col1Ref = _worksheet.Range["BC15"];
        _row2Ref = _worksheet.Range["BD15"];
        _col2Ref = _worksheet.Range["BE15"];
        _ease = _worksheet.Range["BF15"];
        _machinedEdge = _worksheet.Range["BG15"];
        _thickness = _worksheet.Range["ThicknessStart"];
        _material = _worksheet.Range["MaterialStart"];
        _backCut = _worksheet.Range["BJ15"];
        _railSeams = _worksheet.Range["BK15"];
        _grain = _worksheet.Range["BL15"];
        _panelOrientation = _worksheet.Range["OrientationStart"];

        _isInitialized = true;

    }

    public void WriteLine(LineItem line) {

        Initialize();

        WriteToRange(_partNumRef, line.PartNumber);
        WriteToRange(_descriptionRef, line.Description);
        WriteToRange(_qtyRef, line.Qty);
        WriteToRange(_widthRef, line.Width);
        WriteToRange(_heightRef, line.Height);
        WriteToRange(_noteRef, line.SpecialFeatures);
        WriteToRange(_typeRef, line.DoorType);
        WriteToRange(_leftStileRef, line.StileLeft);
        WriteToRange(_rightStileRef, line.StileRight);
        WriteToRange(_topRailRef, line.RailTop);
        WriteToRange(_bottomRailRef, line.RailBottom);
        WriteToRange(_hingeTopRef, line.HingeTop);
        WriteToRange(_hingeBottomRef, line.HingeBottom);
        WriteToRange(_hinge3Ref, line.Hinge3);
        WriteToRange(_hinge4Ref, line.Hinge4);
        WriteToRange(_hinge5Ref, line.Hinge5);
        WriteToRange(_tabRef, line.Tab);
        WriteToRange(_cupDiaRef, line.CupDiameter);
        WriteToRange(_hingePatternRef, line.HingePattern);
        WriteToRange(_cupDepthRef, line.CupDepth);
        WriteToRange(_swingDirRef, line.SwingDirection);
        WriteToRange(_hardwareRefRef, line.HardwareReference);
        WriteToRange(_hardwareRef, line.Hardware);
        WriteToRange(_hardwareTBOffsetRef, line.HardwareTBOffset);
        WriteToRange(_hardwareSideOffsetRef, line.HardwareSideOffset);
        WriteToRange(_hardwareDepthRef, line.HardwareDepth);
        WriteToRange(_doubleHardwareRef, line.DoubleHardware);
        WriteToRange(_panel1Ref, line.Panel1);
        WriteToRange(_rail3Ref, line.RailStile3);
        WriteToRange(_opening2Ref, line.Panel2);
        WriteToRange(_rail4Ref, line.RailStile4);
        WriteToRange(_opening3Ref, line.Panel3);
        WriteToRange(_rail5Ref, line.RailStile5);
        WriteToRange(_rabbetDepth, line.RabbetDepth);
        WriteToRange(_rabbetWidth, line.RabbetWidth);
        WriteToRange(_squareRabbet, line.SquareRabbet);
        WriteToRange(_panelClearance, line.PanelClearance);
        WriteToRange(_panelRadius, line.PanelRadius);
        WriteToRange(_panelThickness, line.PanelThickness);
        WriteToRange(_panelStyle, line.PanelStyle);
        WriteToRange(_mullionOpening, line.MullionOpening);
        WriteToRange(_mullionShape, line.MullionShape);
        WriteToRange(_mullionWidth, line.MullionWidth);
        WriteToRange(_horizMullions, line.HorizontalMullions);
        WriteToRange(_vertMullions, line.VerticalMullions);
        WriteToRange(_row1Ref, line.Row1);
        WriteToRange(_col1Ref, line.Col1);
        WriteToRange(_row2Ref, line.Row2);
        WriteToRange(_col2Ref, line.Col2);
        WriteToRange(_ease, line.Ease);
        WriteToRange(_machinedEdge, line.MachinedEdges);
        WriteToRange(_thickness, line.Thickness);
        WriteToRange(_material, line.Material);
        WriteToRange(_backCut, line.BackCut);
        WriteToRange(_railSeams, line.RailSeams);
        WriteToRange(_grain, line.Grain);
        WriteToRange(_panelOrientation, line.PanelOrientation);

        _offset++;

    }

    private void WriteToRange<T>(Range? rng, Optional<T> data) =>
        data.Switch(
            d => WriteToRange(rng, d),
            none => { });

    private void WriteToRange(Range? rng, object? data) {
        if (rng is null || data is null) return;
        rng.Offset[_offset].Value2 = data;
    }

    private static void ReleaseComObject(object? o) {
        if (o is null) return;
        _ = Marshal.ReleaseComObject(o);
    }

    public void Dispose() {
        GC.SuppressFinalize(this);
        ReleaseComObject(_partNumRef);
        ReleaseComObject(_descriptionRef);
        ReleaseComObject(_qtyRef);
        ReleaseComObject(_widthRef);
        ReleaseComObject(_heightRef);
        ReleaseComObject(_noteRef);
        ReleaseComObject(_typeRef);
        ReleaseComObject(_leftStileRef);
        ReleaseComObject(_rightStileRef);
        ReleaseComObject(_topRailRef);
        ReleaseComObject(_bottomRailRef);
        ReleaseComObject(_hingeTopRef);
        ReleaseComObject(_hingeBottomRef);
        ReleaseComObject(_hinge3Ref);
        ReleaseComObject(_hinge4Ref);
        ReleaseComObject(_hinge5Ref);
        ReleaseComObject(_tabRef);
        ReleaseComObject(_cupDiaRef);
        ReleaseComObject(_hingePatternRef);
        ReleaseComObject(_cupDepthRef);
        ReleaseComObject(_swingDirRef);
        ReleaseComObject(_hardwareRefRef);
        ReleaseComObject(_hardwareRef);
        ReleaseComObject(_hardwareTBOffsetRef);
        ReleaseComObject(_hardwareSideOffsetRef);
        ReleaseComObject(_hardwareDepthRef);
        ReleaseComObject(_doubleHardwareRef);
        ReleaseComObject(_panel1Ref);
        ReleaseComObject(_rail3Ref);
        ReleaseComObject(_opening2Ref);
        ReleaseComObject(_rail4Ref);
        ReleaseComObject(_opening3Ref);
        ReleaseComObject(_rail5Ref);
        ReleaseComObject(_rabbetDepth);
        ReleaseComObject(_rabbetWidth);
        ReleaseComObject(_squareRabbet);
        ReleaseComObject(_panelClearance);
        ReleaseComObject(_panelRadius);
        ReleaseComObject(_panelThickness);
        ReleaseComObject(_panelStyle);
        ReleaseComObject(_mullionOpening);
        ReleaseComObject(_mullionShape);
        ReleaseComObject(_mullionWidth);
        ReleaseComObject(_horizMullions);
        ReleaseComObject(_vertMullions);
        ReleaseComObject(_row1Ref);
        ReleaseComObject(_col1Ref);
        ReleaseComObject(_row2Ref);
        ReleaseComObject(_col2Ref);
        ReleaseComObject(_ease);
        ReleaseComObject(_machinedEdge);
        ReleaseComObject(_thickness);
        ReleaseComObject(_material);
        ReleaseComObject(_backCut);
        ReleaseComObject(_railSeams);
        ReleaseComObject(_grain);
        ReleaseComObject(_panelOrientation);

    }

    ~LineItemWriter() {
        Dispose();
    }

}
