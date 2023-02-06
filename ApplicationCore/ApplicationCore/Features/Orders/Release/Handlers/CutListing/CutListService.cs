using ApplicationCore.Features.Orders.Release.Handlers.CutListing.Models;
using ClosedXML.Excel;

namespace ApplicationCore.Features.Orders.Release.Handlers.CutListing;

public class CutListService {

    private static readonly XLColor HighlightColor = XLColor.FromArgb(191, 191, 191);

    private int _lastRow = 0;

    public IXLWorkbook GenerateCutList(Header header, IEnumerable<PartRow> parts) {

        var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Cut List");
        WriteHeader(header, ws);
        WritePartRows(parts, ws);
        SetPrintArea(ws);

        return wb;

    }

    private void WriteHeader(Header header, IXLWorksheet worksheet) {

        FormatHeaderCell(worksheet.Cell("B1"), "Company");
        FillHeaderValue(worksheet.Range("C1:F1"), header.CustomerName);

        FormatHeaderCell(worksheet.Cell("G1"), "Vendor");
        FillHeaderValue(worksheet.Range("H1:I1"), header.VendorName);

        FormatHeaderCell(worksheet.Cell("B2"), "Order#");
        FillHeaderValue(worksheet.Range("C2:D2"), header.OrderNumber);

        FormatHeaderCell(worksheet.Cell("B3"), "Job Name");
        FillHeaderValue(worksheet.Range("C3:D3"), header.JobName);

        FormatHeaderCell(worksheet.Cell("E2"), "Date");
        FillHeaderValue(worksheet.Range("F2:G2"), header.OrderDate.ToShortDateString());

        FormatHeaderCell(worksheet.Cell("E3"), "Box Count");
        FillHeaderValue(worksheet.Range("F3:G3"), header.BoxCount.ToString());

        FormatHeaderCell(worksheet.Cell("B4"), "Assembly");
        FillHeaderValue(worksheet.Range("C4:E4"), header.Assembly ? "ASSEMBLED" : "*NOT ASSEMBLED*");

        FormatHeaderCell(worksheet.Cell("B5"), "Note");
        FillHeaderValue(worksheet.Range("C5:I5"), header.Note);

        FillHeaderValue(worksheet.Range("H2:I2"), header.Notches);
        FillHeaderValue(worksheet.Range("H3:I3"), $"clips: {header.Clips}");
        FillHeaderValue(worksheet.Range("H4:I4"), $"Mounting Holes: {(header.MountingHoles ? "Yes" : "No")}");
        FillHeaderValue(worksheet.Range("F4:G4"), $"Post Finish: {(header.Finish ? "Yes" : "No")}");

        FormatHeaderCell(worksheet.Cell("A6"), "cab#");
        FormatHeaderCell(worksheet.Cell("B6"), "Part Name");
        FormatHeaderCell(worksheet.Cell("C6"), "Comment");
        FormatHeaderCell(worksheet.Cell("D6"), "Qty");
        FormatHeaderCell(worksheet.Cell("E6"), "Width");
        FormatHeaderCell(worksheet.Cell("F6"), "Length");
        FormatHeaderCell(worksheet.Cell("G6"), "Material");
        FormatHeaderCell(worksheet.Cell("H6"), "Line#");
        FormatHeaderCell(worksheet.Cell("I6"), "Box/Part Size");

        worksheet.Column(2).Width = 10;
        worksheet.Column(3).Width = 30;
        worksheet.Column(5).Width = 10;
        worksheet.Column(7).Width = 10;

        for (int i = 1; i <= 6; i++) {
            worksheet.Row(i).Height = 35;
        }
        double noteRowHeight = Math.Ceiling((header.Note.Length / (double)60)) * 20;
        if (noteRowHeight > 35) {
            worksheet.Row(5).Height = noteRowHeight;
        }

    }

    private void WritePartRows(IEnumerable<PartRow> parts, IXLWorksheet worksheet) {

        var groups = parts.GroupBy(part => part.CabNumbers);

        int row = 7;
        bool highlight = false;
        foreach (var group in groups) {

            foreach (var part in group) {
                worksheet.Cell(row, 1).Value = part.CabNumbers;
                worksheet.Cell(row, 2).Value = part.Name;
                worksheet.Cell(row, 3).Value = part.Comment;
                worksheet.Cell(row, 4).Value = part.Qty;
                worksheet.Cell(row, 5).Value = part.Width;
                worksheet.Cell(row, 6).Value = part.Length;
                worksheet.Cell(row, 7).Value = part.Material;
                worksheet.Cell(row, 8).Value = part.Line;
                worksheet.Cell(row, 9).Value = part.PartSize;

                worksheet.Range(row, 4, row, 6).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                worksheet.Cell(row, 8).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                row = row + 1;
            }

            var rng = worksheet.Range(row - group.Count(), 1, row - 1, 9);
            rng.Style.Alignment.SetWrapText(true);
            rng.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            if (highlight) {
                rng.Style.Fill.BackgroundColor = HighlightColor;
            }

            highlight = !highlight;

        }

        _lastRow = row;

        worksheet.Column(9).AdjustToContents();

    }

    private void SetPrintArea(IXLWorksheet worksheet) {

        worksheet.PageSetup.PrintAreas.Add(1, 1, _lastRow, 9);
        worksheet.PageSetup.SetPageOrientation(XLPageOrientation.Landscape);
        worksheet.PageSetup.PagesWide = 1;
    }

    private static void FormatHeaderCell(IXLCell cell, string value) {

        cell.Value = value;
        cell.Style.Fill.BackgroundColor = HighlightColor;
        cell.Style.Alignment.SetWrapText(true);
        cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        cell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        cell.Style.Border.SetTopBorder(XLBorderStyleValues.Thin);
        cell.Style.Border.SetBottomBorder(XLBorderStyleValues.Thin);
        cell.Style.Border.SetLeftBorder(XLBorderStyleValues.Thin);
        cell.Style.Border.SetRightBorder(XLBorderStyleValues.Thin);

    }

    private static void FillHeaderValue(IXLRange range, string value) {

        range.Cell(1, 1).Value = value;
        if (range.Cells().Count() > 1) {
            range.Merge();
        }
        range.Style.Alignment.SetWrapText(true);
        range.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        range.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        range.Style.Border.SetTopBorder(XLBorderStyleValues.Thin);
        range.Style.Border.SetBottomBorder(XLBorderStyleValues.Thin);
        range.Style.Border.SetLeftBorder(XLBorderStyleValues.Thin);
        range.Style.Border.SetRightBorder(XLBorderStyleValues.Thin);

    }

}