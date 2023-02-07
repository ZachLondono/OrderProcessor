using ApplicationCore.Features.Orders.Release.Handlers.PackingList.Models;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ApplicationCore.Features.Orders.Release.Handlers.PackingList;

internal class PackingListService {

    private static readonly XLColor HighlightColor = XLColor.FromArgb(242, 242, 242);

    public IXLWorkbook GeneratePackingList(Models.PackingList packingList) {

        var workbook = new XLWorkbook();

        var ws = workbook.Worksheets.Add("Packing List");

        var titleRng = ws.Range("A1:H1");
        titleRng.Cell(1, 1).Value = "Packing List";
        titleRng.Style.Font.FontSize = 48;
        titleRng.Merge();
        titleRng.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Bottom);
        titleRng.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        ws.Row(1).Height = 62;

        var fromCell = ws.Cell("B2");
        fromCell.Value = "From: ";
        fromCell.Style.Font.SetItalic(true);
        fromCell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Bottom);
        fromCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

        AddCompanyInfo(packingList.Vendor, fromCell.CellRight());

        var toCell = ws.Cell("B7");
        toCell.Value = "To: ";
        toCell.Style.Font.SetItalic(true);
        toCell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Bottom);
        toCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

        AddCompanyInfo(packingList.Customer, toCell.CellRight());

        AddInfoField(ws.Cell("F2"), "Date", packingList.Date.ToShortDateString());
        ws.Range("G2:H2").Merge();
        AddInfoField(ws.Cell("F4"), "Tracking #:", packingList.OrderNumber);
        AddInfoField(ws.Cell("F5"), "Name:", packingList.OrderName);

        ws.Row(6).Height = 7;
        ws.Row(11).Height = 7;

        int row = 12;

        if (packingList.DrawerBoxes.Any()) {
            AddDrawerBoxTable(ws, row, packingList.DrawerBoxes);
        }

        if (packingList.Doors.Any()) {
            AddDoorTable(ws, row, packingList.Doors);
        }

        if (packingList.Cabinets.Any()) {
            AddCabinetTable(ws, row, packingList.Cabinets);
        }

        ws.Column(4).Width = 20;

        return workbook;

    }

    public static void AddInfoField(IXLCell cell, string header, object value) {

        cell.Value = header;
        cell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Bottom);
        cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

        cell = cell.CellRight();
        cell.Value = value;
        cell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Bottom);
        cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

    }

    public static void AddCompanyInfo(Company company, IXLCell cell) {

        cell.Value = company.Name;
        cell.Style.Font.SetFontSize(16);
        cell.Style.Font.SetBold(true);

        cell = cell.CellBelow();
        cell.Value = company.Line1;
        
        cell = cell.CellBelow();
        cell.Value = company.Line2;

        cell = cell.CellBelow();
        cell.Value = company.Line3;

    }

    private static void AddProductTitile(IXLWorksheet worksheet, int row, int qty, string message) {

        var countCell = worksheet.Cell(row, 2);
        countCell.Value = qty;
        countCell.Style.Fill.BackgroundColor = HighlightColor;
        countCell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        countCell.Style.Font.SetFontSize(14);

        var titleCell = worksheet.Cell(row, 3);
        titleCell.Value = message;
        titleCell.Style.Border.SetLeftBorder(XLBorderStyleValues.Thin);
        titleCell.Style.Border.SetTopBorder(XLBorderStyleValues.Thin);
        titleCell.Style.Border.SetBottomBorder(XLBorderStyleValues.Thin);
        titleCell.Style.Font.SetFontSize(14);
        var titleCell2 = worksheet.Cell(row, 4);
        titleCell2.Style.Border.SetRightBorder(XLBorderStyleValues.Thin);
        titleCell2.Style.Border.SetTopBorder(XLBorderStyleValues.Thin);
        titleCell2.Style.Border.SetBottomBorder(XLBorderStyleValues.Thin);

    }

    private static void AddHeader(IXLCell cell, string value) {

        cell.Value = value;
        cell.Style.Font.SetBold(true);
        cell.Style.Fill.BackgroundColor = HighlightColor;
        cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

    }

    private static void AddRowCell(IXLCell cell, object value) {

        cell.Value = value;
        cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

    }

    private static void AddDescriptionCell(IXLWorksheet worksheet, IXLCell cell, object value) {

        cell.Value = value;
        worksheet.Range(cell, cell.CellRight()).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
    }

    public static int AddDrawerBoxTable(IXLWorksheet worksheet, int row, IEnumerable<DrawerBoxItem> drawerBoxes) {

        AddProductTitile(worksheet, row, drawerBoxes.Count(), "Drawer Box(es) in order");

        worksheet.Row(++row).Height = 7;

        ++row;
        AddHeader(worksheet.Cell(row, 2), "Cab");
        AddHeader(worksheet.Cell(row, 3), "Qty");
        AddHeader(worksheet.Cell(row, 6), "Width");
        AddHeader(worksheet.Cell(row, 7), "Height");
        AddHeader(worksheet.Cell(row, 8), "Depth");

        var desc = worksheet.Range(row, 4, row, 5);
        desc.Cell(1, 1).Value = "Description";
        desc.Merge();
        desc.Style.Font.SetBold(true);
        desc.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        desc.Style.Fill.BackgroundColor = HighlightColor;
        desc.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

        foreach (var drawerBoxItem in drawerBoxes) {

            ++row;
            AddRowCell(worksheet.Cell(row, 2), drawerBoxItem.Line);
            AddRowCell(worksheet.Cell(row, 3), drawerBoxItem.Qty);
            AddDescriptionCell(worksheet, worksheet.Cell(row, 4), drawerBoxItem.Description);
            AddRowCell(worksheet.Cell(row, 6), drawerBoxItem.Width);
            AddRowCell(worksheet.Cell(row, 7), drawerBoxItem.Height);
            AddRowCell(worksheet.Cell(row, 8), drawerBoxItem.Depth);

        }

        return row;

    }

    public static int AddDoorTable(IXLWorksheet worksheet, int row, IEnumerable<DoorItem> doors) {
        
        AddProductTitile(worksheet, row, doors.Count(), "Door(s) in order");
        
        worksheet.Row(++row).Height = 7;

        ++row;
        AddHeader(worksheet.Cell(row, 2), "Cab");
        AddHeader(worksheet.Cell(row, 3), "Qty");
        AddHeader(worksheet.Cell(row, 6), "Width");
        AddHeader(worksheet.Cell(row, 7), "Height");

        var desc = worksheet.Range(row, 4, row, 5);
        desc.Cell(1,1).Value = "Description";
        desc.Merge();
        desc.Style.Font.SetBold(true);
        desc.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        desc.Style.Fill.BackgroundColor = HighlightColor;
        desc.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

        foreach (var doorItem in doors) {

            ++row;
            AddRowCell(worksheet.Cell(row, 2), doorItem.Line);
            AddRowCell(worksheet.Cell(row, 3), doorItem.Qty);
            AddDescriptionCell(worksheet, worksheet.Cell(row, 4), doorItem.Description);
            AddRowCell(worksheet.Cell(row, 6), doorItem.Width);
            AddRowCell(worksheet.Cell(row, 7), doorItem.Height);

        }

        return row;

    }

    public static int AddCabinetTable(IXLWorksheet worksheet, int row, IEnumerable<CabinetItem> cabinets) {

        AddProductTitile(worksheet, row, cabinets.Count(), "Cabinet(s) in order");
        
        worksheet.Row(++row).Height = 7;

        ++row;
        AddHeader(worksheet.Cell(row, 2), "Cab");
        AddHeader(worksheet.Cell(row, 3), "Qty");
        AddHeader(worksheet.Cell(row, 6), "Width");
        AddHeader(worksheet.Cell(row, 7), "Height");
        AddHeader(worksheet.Cell(row, 8), "Depth");

        var desc = worksheet.Range(row, 4, row, 5);
        desc.Cell(1, 1).Value = "Description";
        desc.Merge();
        desc.Style.Font.SetBold(true);
        desc.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        desc.Style.Fill.BackgroundColor = HighlightColor;
        desc.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

        foreach (var cabinetItem in cabinets) {

            ++row;
            AddRowCell(worksheet.Cell(row, 2), cabinetItem.Line);
            AddRowCell(worksheet.Cell(row, 3), cabinetItem.Qty);
            AddDescriptionCell(worksheet, worksheet.Cell(row, 4), cabinetItem.Description);
            AddRowCell(worksheet.Cell(row, 6), cabinetItem.Width);
            AddRowCell(worksheet.Cell(row, 7), cabinetItem.Height);
            AddRowCell(worksheet.Cell(row, 8), cabinetItem.Depth);

        }

        return row;

    }

}
