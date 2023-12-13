using ApplicationCore.Features.Orders.OrderLoading.LoadDoweledDBSpreadsheetOrderData.Models;
using Microsoft.Office.Interop.Excel;
using ExcelApplication = Microsoft.Office.Interop.Excel.Application;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadTailoredLivingSpreadsheetOrderData;

public class TailoredLivingOrderReader {

    private ExcelApplication? _app = null;

    public Order? ReadOrderFromFile(string filepath) {
              
        Workbook? workbook = OpenWorkbook(filepath);
        
        if (workbook is null) {
        
            return null;
        
        }
        
        var cover = ReadCoverPage(workbook);
        
        if (cover is null) {
        
            return null;
        
        }
        
        var rooms = ReadRooms(workbook);
        
        if (rooms.Length == 0) {
        
            return null;
        
        }
        
        return new(cover, rooms);

    }
    
    private static CoverPage? ReadCoverPage(Workbook workbook) {
    
        Worksheet? coverSheet = (Worksheet?) workbook.Worksheets["Cover"];
        
        if (coverSheet is null) return null;
    
        string jobName = coverSheet.GetRangeStringValue("B2");
    
        string closetPartCheckBox = coverSheet.GetRangeStringValue("B5");
        string garageCabinetsCheckBox = coverSheet.GetRangeStringValue("E5");
        string stdCabinetsCheckBox = coverSheet.GetRangeStringValue("H5");

        string stdInteriorCheckBox = coverSheet.GetRangeStringValue("B8");
        string stdInteriorColor = coverSheet.GetRangeStringValue("C6");
        string otherInteriorCheckBox = coverSheet.GetRangeStringValue("E8");
        string otherInteriorColor = coverSheet.GetRangeStringValue("H7");
        
        string interiorMaterial = (string.IsNullOrWhiteSpace(stdInteriorCheckBox) && !string.IsNullOrWhiteSpace(otherInteriorCheckBox)) ? otherInteriorColor : stdInteriorColor;

        string stdExteriorCheckBox = coverSheet.GetRangeStringValue("B11");
        string stdExteriorColor = coverSheet.GetRangeStringValue("C9");
        string otherExteriorCheckBox = coverSheet.GetRangeStringValue("E11");
        string otherExteriorColor = coverSheet.GetRangeStringValue("H10");
        
        string exteriorMaterial = (string.IsNullOrWhiteSpace(stdExteriorCheckBox) && !string.IsNullOrWhiteSpace(otherExteriorCheckBox)) ? otherExteriorColor : stdExteriorColor;

        string noToeCheckBox = coverSheet.GetRangeStringValue("B14");
        string notchToeCheckBox = coverSheet.GetRangeStringValue("E14");
        string notchToeSize = coverSheet.GetRangeStringValue("F13");
        string legLevelerCheckBox = coverSheet.GetRangeStringValue("H14");
        
        string confirmatFinishCheckBox = coverSheet.GetRangeStringValue("B17");
        string noConfirmatFinishCheckBox = coverSheet.GetRangeStringValue("E17");
        string appliedPanelFinishCheckBox = coverSheet.GetRangeStringValue("H17");
        
        string dovetailDBCheckBox = coverSheet.GetRangeStringValue("B20");
        string otherDBCheckBox = coverSheet.GetRangeStringValue("E20");
        string otherDBTypeName = coverSheet.GetRangeStringValue("I18");
        
        string counter1MatThickness = coverSheet.GetRangeStringValue("B22");
        string counter1Edge = coverSheet.GetRangeStringValue("E22");
        string counter1FinishedEdges = coverSheet.GetRangeStringValue("H22");
        
        string counter2MatThickness = coverSheet.GetRangeStringValue("B25");
        string counter2Edge = coverSheet.GetRangeStringValue("E25");
        string counter2FinishedEdges = coverSheet.GetRangeStringValue("H25");
        
        string standarLeadTimeCheckBox = coverSheet.GetRangeStringValue("B29");
        string rushCheckBox = coverSheet.GetRangeStringValue("E29");
        string requestedDateText = coverSheet.GetRangeStringValue("H28");

        DateTime? requestedDate = null;
        if (DateTime.TryParse(requestedDateText, out DateTime parsedDate)) {
            requestedDate = parsedDate;
        }

        string additionalNotes = coverSheet.GetRangeStringValue("A22");
        
        var counterTop1 = new CounterTop(counter1MatThickness, counter1Edge, counter1FinishedEdges);
        var counterTop2 = new CounterTop(counter2MatThickness, counter2Edge, counter2FinishedEdges);
        
        return new CoverPage(
            JobName:                    jobName,
            ClosetParts:                !string.IsNullOrWhiteSpace(closetPartCheckBox),
            GarageCabinets:             !string.IsNullOrWhiteSpace(garageCabinetsCheckBox),
            StandardCabinets:           !string.IsNullOrWhiteSpace(stdCabinetsCheckBox),
            InteriorMaterial:           interiorMaterial,
            ExteriorMaterial:           exteriorMaterial,
            NoToe:                      !string.IsNullOrWhiteSpace(noToeCheckBox),
            LegLevelers:                !string.IsNullOrWhiteSpace(legLevelerCheckBox),
            ToeNotch:                   !string.IsNullOrWhiteSpace(notchToeCheckBox),
            ToeNotchSize:               notchToeSize,
            FinishedWithConfirmats:     !string.IsNullOrWhiteSpace(confirmatFinishCheckBox),
            FinishedWithoutConfirmats:  !string.IsNullOrWhiteSpace(noConfirmatFinishCheckBox),
            FinishedWithAppliedPanels:  !string.IsNullOrWhiteSpace(appliedPanelFinishCheckBox),
            DovetailDrawerBoxes:        !string.IsNullOrWhiteSpace(dovetailDBCheckBox),
            OtherDrawerBoxType:         otherDBTypeName,
            Counter1:                   counterTop1,
            Counter2:                   counterTop2,
            Rush:                       !string.IsNullOrWhiteSpace(rushCheckBox),
            RequestedDate:              requestedDate,
            AdditionalNotes:            additionalNotes
        );
    
    }
    
    private static Room[] ReadRooms(Workbook workbook) {
    
        var specialSheetNames = new string[] {
            "Cover",
            "Garage",
            "Std. Cabinet"        
        };
    
        List<Room> rooms = new();
    
        foreach (Worksheet sheet in workbook.Worksheets) {
        
            if (specialSheetNames.Contains(sheet.Name)
                || !IsOrderSheetHeaderValid(sheet)) continue;
            
            List<Item> items = new();
            
            int row = 2;
            while (true) {
            
                var description =       sheet.GetRangeStringValue("A" + row);
                var partNumber =        sheet.GetRangeStringValue("B" + row);
                var qtyStr =            sheet.GetRangeStringValue("C" + row);
                var widthStr =          sheet.GetRangeStringValue("D" + row);
                var heightStr =         sheet.GetRangeStringValue("E" + row);
                var depthStr =          sheet.GetRangeStringValue("F" + row);
                var dim4Str =           sheet.GetRangeStringValue("G" + row);
                var costStr =           sheet.GetRangeStringValue("H" + row);
                var laborStr =          sheet.GetRangeStringValue("I" + row);
                var totalChargeStr =    sheet.GetRangeStringValue("J" + row);
                var notes =             sheet.GetRangeStringValue("K" + row);

                if (!int.TryParse(qtyStr, out int qty)) qty = 0;
                if (!double.TryParse(widthStr, out double width)) width = 0;
                if (!double.TryParse(heightStr, out double height)) height = 0;
                if (!double.TryParse(depthStr, out double depth)) depth = 0;
                if (!double.TryParse(dim4Str, out double dim4)) dim4 = 0;
                if (!decimal.TryParse(costStr.Remove('$'), out decimal cost)) cost = 0;
                if (!decimal.TryParse(laborStr.Remove('$'), out decimal labor)) labor = 0;
                if (!decimal.TryParse(totalChargeStr.Remove('$'), out decimal totalCharge)) totalCharge = 0;

                items.Add(new(
                    Description: description,
                    PartNumber: partNumber,
                    Qty: qty,
                    Width: width,
                    Height: height,
                    Depth: depth,
                    Dim4: dim4,
                    Cost: cost,
                    LaborCharge: labor,
                    TotalCharge: totalCharge,
                    SpecialNotes: notes
                ));

                row++;
                        
            }

        }
        
        return rooms.ToArray();
    
    }
    
    private static bool IsOrderSheetHeaderValid(Worksheet sheet) {
    
        return sheet.GetRangeStringValue("A1").Equals("Description")
        && sheet.GetRangeStringValue("B1").Equals("Part Number")
        && sheet.GetRangeStringValue("C1").Equals("# of Items in Design")
        && sheet.GetRangeStringValue("D1").Equals(  """
                                                    Cut 
                                                    Width
                                                    """)
        && sheet.GetRangeStringValue("E1").Equals(  """
                                                    Cut 
                                                    Height
                                                    """)
        && sheet.GetRangeStringValue("F1").Equals(  """
                                                    Cut 
                                                    Depth
                                                    """)
        && sheet.GetRangeStringValue("G1").Equals(  """
                                                    Cut 
                                                    Dim 4
                                                    """)
        && sheet.GetRangeStringValue("H1").Equals("Cost")
        && sheet.GetRangeStringValue("I1").Equals("Labor Charge")
        && sheet.GetRangeStringValue("J1").Equals("Total Charge")
        && sheet.GetRangeStringValue("K1").Equals("Special Notes");
    
    }
    
    private Workbook? OpenWorkbook(string filepath) {
    
        try {
        
            _app ??= new ExcelApplication() {
                            Visible = false
                        };
            
            return _app.Workbooks.Open(filepath);
            
        } catch {
        
            return null;
        
        }
    
    }
    
}

