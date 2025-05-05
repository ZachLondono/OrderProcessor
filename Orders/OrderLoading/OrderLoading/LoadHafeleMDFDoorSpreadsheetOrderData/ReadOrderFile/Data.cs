using ClosedXML.Excel;

namespace OrderLoading.LoadHafeleMDFDoorSpreadsheetOrderData.ReadOrderFile;

public class Data {

    public double HafeleMarkUpToCustomers { get; init; }
    public double DiscountToHafele { get; init; }
    public Dictionary<string, double> MaterialThicknessesByName { get; init; } = [];
    public Dictionary<string, int> PanelCountByDoorType { get; init; } = [];

    public static Data LoadFromWorkbook(XLWorkbook workbook) {

        var sheet = workbook.Worksheet("Data");

        var markUp = sheet.Cell("Hafele_Mark_Up").GetDouble();
        var discount = sheet.Cell("Discount").GetDouble();

        var (materials, thicknesses) = GetMaterialData(sheet);
        Dictionary<string, double> thicknessesByMaterial = [];
        for (int i = 0; i < materials.Length; i++) {
            thicknessesByMaterial[materials[i]] = thicknesses[i];
        }

        var (doorTypes, doorPanelCounts) = GetDoorTypeData(sheet);
        Dictionary<string, int> panelCountByDoorTypes = [];
        for (int i = 0; i < doorTypes.Length; i++) {
            panelCountByDoorTypes[doorTypes[i]] = doorPanelCounts[i];
        }

        return new Data() {
            HafeleMarkUpToCustomers = markUp,
            DiscountToHafele = discount,
            MaterialThicknessesByName = thicknessesByMaterial,
            PanelCountByDoorType = panelCountByDoorTypes
        };

    }

    private static (string[] MaterialNames, double[] MaterialThicknesses) GetMaterialData(IXLWorksheet sheet) {

        var materials = sheet.Cells("Materials")
                             .Select(c => c.GetValue<string>())
                             .Where(m => !string.IsNullOrEmpty(m))
                             .ToArray();

        int expectedRowStart = 10;
        int expectedRowEnd = expectedRowStart + materials.Length - 1;

        var thicknesses = sheet.Cells($"Q{expectedRowStart}:Q{expectedRowEnd}")
                                .Select(c => c.GetValue<string>())
                                .Where(t => !string.IsNullOrEmpty(t))
                                .Select(t => double.Parse(t))
                                .ToArray();

        return (materials, thicknesses);

    }

    private static (string[] DoorTypes, int[] PanelCounts) GetDoorTypeData(IXLWorksheet sheet) {

        IXLCells doorTypesRange;

        try {

            doorTypesRange = sheet.Cells("Door_Types");

        } catch {

            doorTypesRange = sheet.Cells("B10:B23");

        }

        var doorTypes = doorTypesRange.Select(c => c.GetValue<string>())
                                      .Where(v => !string.IsNullOrWhiteSpace(v))
                                      .ToArray();


        IXLCells panelCountRange;

        try {

            panelCountRange = sheet.Cells("Door_Types_Panel_Counts");

        } catch {

            int expectedRowStart = 10;
            int expectedRowEnd = expectedRowStart + doorTypes.Length - 1;

            panelCountRange = sheet.Cells($"D{expectedRowStart}:D{expectedRowEnd}");

        }

        var doorPanelCounts = panelCountRange.Select(c => c.GetValue<string>())
                                   .Where(v => !string.IsNullOrWhiteSpace(v))
                                   .Select(v => int.Parse(v))
                                   .ToArray();

        return (doorTypes, doorPanelCounts);

    }

}
