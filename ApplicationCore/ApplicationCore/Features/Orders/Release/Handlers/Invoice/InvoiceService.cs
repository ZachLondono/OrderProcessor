using ApplicationCore.Features.Orders.Release.Handlers.Invoice.Models;
using ClosedXML.Excel;

namespace ApplicationCore.Features.Orders.Release.Handlers.Invoice;

internal class InvoiceService {

    private static readonly XLColor HighlightColor = XLColor.FromArgb(206, 213, 234);

    public IXLWorkbook GenerateInvoice(Models.Invoice invoice) {

        var workbook = new XLWorkbook();

        var ws = workbook.Worksheets.Add("Invoice");

        var titleRng = ws.Range("A1:J1");
        titleRng.Cell(1, 1).Value = "Invoice";
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

        AddCompanyInfo(invoice.Vendor, fromCell.CellRight());

        var toCell = ws.Cell("B7");
        toCell.Value = "To: ";
        toCell.Style.Font.SetItalic(true);
        toCell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Bottom);
        toCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
        AddCompanyInfo(invoice.Customer, toCell.CellRight());

        AddInfoField(ws.Cell("H2"), "Date", invoice.Date.ToShortDateString());
        ws.Range("I2:J2").Merge();
        AddInfoField(ws.Cell("H4"), "Tracking #:", invoice.OrderNumber);
        AddInfoField(ws.Cell("H5"), "Name:", invoice.OrderName);

        AddInfoField(ws.Cell("I8"), "Subtotal", invoice.SubTotal.ToString("$0.00"));
        int row = 9;
        if (invoice.Discount != 0) {
            AddInfoField(ws.Cell($"I{row++}"), "Discount", invoice.Discount.ToString("0.00"));
            AddInfoField(ws.Cell($"I{row++}"), "Net Amt.", invoice.NetAmount.ToString("0.00"));
        }
        AddInfoField(ws.Cell($"I{row++}"), "Sales Tax", invoice.SalesTax.ToString("0.00"));
        AddInfoField(ws.Cell($"I{row++}"), "Shipping", invoice.Shipping.ToString("0.00"));
        var totalCell = ws.Cell($"J{row}");
        totalCell.Value = invoice.Total.ToString("0.00");
        totalCell.Style.Font.SetBold(true);
        totalCell.Style.Border.SetTopBorder(XLBorderStyleValues.Thin);
        ws.Range($"I8:J{row}").Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

        ws.Row(6).Height = 7;
        ws.Row(row+1).Height = 7;
        row += 2;

        if (invoice.DrawerBoxes.Any()) {
            row = AddDrawerBoxTable(ws, row, invoice.DrawerBoxes);
        }

        if (invoice.Doors.Any()) {
            row = AddDoorTable(ws, row, invoice.Doors);
        }

        if (invoice.ClosetParts.Any()) {
            // Do something...
        }

        if (invoice.Cabinets.Any()) {
            row = AddCabinetTable(ws, row, invoice.Cabinets);
        }

        ws.Column(4).Width = 20;

        ws.PageSetup.PrintAreas.Add(1, 1, row, 10);
        ws.PageSetup.PagesWide = 1;

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

    private static void AddProductTitle(IXLWorksheet worksheet, int row, int qty, string message) {

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

        AddProductTitle(worksheet, row, drawerBoxes.Sum(d => d.Qty), "Drawer Box(es) in order");

        worksheet.Row(++row).Height = 7;

        ++row;
        AddHeader(worksheet.Cell(row, 2), "Cab");
        AddHeader(worksheet.Cell(row, 3), "Qty");
        AddHeader(worksheet.Cell(row, 6), "Width");
        AddHeader(worksheet.Cell(row, 7), "Height");
        AddHeader(worksheet.Cell(row, 8), "Depth");
        AddHeader(worksheet.Cell(row, 9), "Price");
        AddHeader(worksheet.Cell(row, 10), "Ext. Price");

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
            AddRowCell(worksheet.Cell(row, 9), drawerBoxItem.Price);
            AddRowCell(worksheet.Cell(row, 10), drawerBoxItem.ExtPrice);

        }

        return row;

    }

    public static int AddDoorTable(IXLWorksheet worksheet, int row, IEnumerable<DoorItem> doors) {

        AddProductTitle(worksheet, row, doors.Sum(d => d.Qty), "Door(s) in order");

        worksheet.Row(++row).Height = 7;

        ++row;
        AddHeader(worksheet.Cell(row, 2), "Cab");
        AddHeader(worksheet.Cell(row, 3), "Qty");
        AddHeader(worksheet.Cell(row, 6), "Width");
        AddHeader(worksheet.Cell(row, 7), "Height");
        AddHeader(worksheet.Cell(row, 8), "Price");
        AddHeader(worksheet.Cell(row, 9), "Ext. Price");

        var desc = worksheet.Range(row, 4, row, 5);
        desc.Cell(1, 1).Value = "Description";
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
            AddRowCell(worksheet.Cell(row, 9), doorItem.Price);
            AddRowCell(worksheet.Cell(row, 10), doorItem.ExtPrice);

        }

        return row;

    }

    public static int AddCabinetTable(IXLWorksheet worksheet, int row, IEnumerable<CabinetItem> cabinets) {

        AddProductTitle(worksheet, row, cabinets.Sum(c => c.Qty), "Cabinet(s) in order");

        worksheet.Row(++row).Height = 7;

        ++row;
        AddHeader(worksheet.Cell(row, 2), "Cab");
        AddHeader(worksheet.Cell(row, 3), "Qty");
        AddHeader(worksheet.Cell(row, 6), "Width");
        AddHeader(worksheet.Cell(row, 7), "Height");
        AddHeader(worksheet.Cell(row, 8), "Depth");
        AddHeader(worksheet.Cell(row, 9), "Price");
        AddHeader(worksheet.Cell(row, 10), "Ext. Price");

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
            AddRowCell(worksheet.Cell(row, 9), cabinetItem.Price);
            AddRowCell(worksheet.Cell(row, 10), cabinetItem.ExtPrice);

        }

        return row;

    }

}
