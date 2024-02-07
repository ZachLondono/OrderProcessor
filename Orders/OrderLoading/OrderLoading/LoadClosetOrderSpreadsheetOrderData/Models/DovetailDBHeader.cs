using Microsoft.Office.Interop.Excel;

namespace OrderLoading.LoadClosetOrderSpreadsheetOrderData.Models;

public record DovetailDBHeader(string BoxMaterial, string BottomMaterial, string Notch, string Clips, bool PostFinish, bool SandFlush, bool IncludeHettichSlides) {

    public static DovetailDBHeader ReadFromWorksheet(Worksheet worksheet) {

        // Side Material
        var solidBirch = worksheet.CheckBoxes("SolidCheckBox").Value == 1;
        var economyBirch = worksheet.CheckBoxes("EconomyCheckBox").Value == 1;
        var balticBirch = worksheet.CheckBoxes("BalticPlyCheckBox").Value == 1;
        var hybridBirch = worksheet.CheckBoxes("HybridCheckBox").Value == 1;

        string boxMaterial = "UNKNOWN";
        if (solidBirch) boxMaterial = "Solid Birch";
        if (economyBirch) boxMaterial = "Economy Birch";
        if (balticBirch) boxMaterial = "Baltic Birch Ply";
        if (hybridBirch) boxMaterial = "Hybrid Birch";

        // Bottom Thickness
        var qtrInch = worksheet.CheckBoxes("QtrCheckBox").Value == 1;
        var halfInch = worksheet.CheckBoxes("HalfCheckBox").Value == 1;

        // Bottom Material
        var plywood = worksheet.CheckBoxes("PlyCheckBox").Value == 1;
        var whiteMela = worksheet.CheckBoxes("WhiteCheckBox").Value == 1;
        var blackMela = worksheet.CheckBoxes("BlackCheckBox").Value == 1;

        string bottomMaterial = halfInch ? "1/2\" " : "1/2\" ";
        if (plywood) bottomMaterial += "Plywood";
        if (whiteMela) bottomMaterial += "White Melamine";
        if (blackMela) bottomMaterial += "Black Melamine";

        // Notch
        var stdNotch = worksheet.CheckBoxes("StdCheckBox").Value == 1;
        var wideNotch = worksheet.CheckBoxes("WideCheckBox").Value == 1;
        var notch828 = worksheet.CheckBoxes("Notch828CheckBox").Value == 1;

        string notch = "No Notch";
        if (stdNotch) notch = "Notch for U/M Slide";
        if (wideNotch) notch = "Notch for U/M Slide-Wide";
        if (notch828) notch = "828 Notch";

        // Clips
        var blum = worksheet.CheckBoxes("BlumCheckBox").Value == 1;
        var hettich = worksheet.CheckBoxes("HettichCheckBox").Value == 1;
        var richelieu = worksheet.CheckBoxes("RichCheckBox").Value == 1;

        string clips = "None";
        if (blum) clips = "Blum";
        if (hettich) clips = "Hettich";
        if (richelieu) clips = "Richelieu";

        var includeHettichSlides = false;
        var boxes = worksheet.CheckBoxes();
        foreach (CheckBox box in boxes) {
            if (box.Name == "HettichSlideCheckBox") {
                includeHettichSlides = worksheet.CheckBoxes("HettichSlideCheckBox").Value == 1;
            }
        }

        var postFinish = worksheet.CheckBoxes("PostCheckBox").Value == 1;
        var sandFlush = worksheet.CheckBoxes("SandCheckBox").Value == 1;

        return new(boxMaterial, bottomMaterial, notch, clips, postFinish, sandFlush, includeHettichSlides);

    }

}

