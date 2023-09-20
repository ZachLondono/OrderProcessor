using ApplicationCore.Features.Orders.Shared.Domain.Products.DrawerBoxes;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;
using Microsoft.Office.Interop.Excel;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadDoweledDBSpreadsheetOrderData.Models;

public record LineItem {

    public int Number { get; init; }
    public string Note { get; init; } = string.Empty;
    public int Qty { get; init; }
    public double Height { get; init; }
    public double Width { get; init; }
    public double Depth { get; init; }
    public string Instructions { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public string FrontBackColor { get; init; } = string.Empty;
    public double FrontBackThickness { get; init; }
    public bool FrontBackGrained { get; init; }
    public string SidesColor { get; init; } = string.Empty;
    public double SidesThickness { get; init; }
    public bool SidesGrained { get; init; }
    public string BottomColor { get; init; } = string.Empty;
    public double BottomThickness { get; init; }
    public bool BottomGrained { get; init; }

    public static bool DoesRowContainItem(Worksheet worksheet, int row) {

        var lineVal = worksheet.GetRangeStringValue("BoxNumStart", row);
        if (lineVal == "") {
            return false;
        }

        if (int.TryParse(lineVal, out int num)) {
            return num != 0;
        }

        return false;

    }

    public static LineItem ReadFromSheet(Worksheet worksheet, int row) {

        return new() {
            Number = (int)worksheet.GetRangeValue<double>("BoxNumStart", 0, row),
            Note = worksheet.GetRangeStringValue("BoxNoteStart", row),
            Qty = (int)worksheet.GetRangeValue<double>("BoxQtyStart", 0, row),
            Height = worksheet.GetRangeValue<double>("BoxHeightStart", 0, row),
            Width = worksheet.GetRangeValue<double>("BoxWidthStart", 0, row),
            Depth = worksheet.GetRangeValue<double>("BoxDepthStart", 0, row),
            Instructions = worksheet.GetRangeStringValue("BoxInstructionsStart", row),
            UnitPrice = (decimal)worksheet.GetRangeValue<double>("BoxUnitPriceStart", 0, row),
            FrontBackColor = worksheet.GetRangeStringValue("FrontBackColorStart", row),
            FrontBackThickness = worksheet.GetRangeValue<double>("FrontBackThicknessStart", 0, row),
            FrontBackGrained = worksheet.GetRangeStringValue("FrontBackGrainedStart", row) == "Yes",
            SidesColor = worksheet.GetRangeStringValue("SidesColorStart", row),
            SidesThickness = worksheet.GetRangeValue<double>("SidesThicknessStart", 0, row),
            SidesGrained = worksheet.GetRangeStringValue("SidesGrainedStart", row) == "Yes",
            BottomColor = worksheet.GetRangeStringValue("BottomColorStart", row),
            BottomThickness = worksheet.GetRangeValue<double>("BottomThicknessStart", 0, row),
            BottomGrained = worksheet.GetRangeStringValue("BottomGrainedStart", row) == "Yes"
        };

    }

    public DoweledDrawerBoxProduct CreateDoweledDrawerBoxProduct(bool useInches, bool machineThicknessForUMSlides, Dimension frontBackHeightAdjustment) {

        var id = Guid.NewGuid();
        var room = "";

        Func<double, Dimension> dimParse = useInches switch {
            true => Dimension.FromInches,
            false => Dimension.FromMillimeters,
        };

        Dimension height = dimParse(Height);
        Dimension width = dimParse(Width);
        Dimension depth = dimParse(Depth);
        Dimension frontBackThickness = dimParse(FrontBackThickness);
        Dimension sideThickness = dimParse(SidesThickness);
        Dimension bottomThickness = dimParse(BottomThickness);

        var frontBackMaterial = new DoweledDrawerBoxMaterial(FrontBackColor, frontBackThickness, FrontBackGrained);
        var sidesMaterial = new DoweledDrawerBoxMaterial(SidesColor, sideThickness, SidesGrained);
        var bottomMaterial = new DoweledDrawerBoxMaterial(BottomColor, bottomThickness, BottomGrained);

        return new DoweledDrawerBoxProduct(id, UnitPrice, Qty, room, Number, height, width, depth, frontBackMaterial, frontBackMaterial, sidesMaterial, bottomMaterial, machineThicknessForUMSlides, frontBackHeightAdjustment);

    }

}
