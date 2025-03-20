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

        var materials = sheet.Cells("Materials").Select(c => c.GetValue<string>()).Where(m => !string.IsNullOrEmpty(m)).ToArray();
        var thicknesses = sheet.Cells("Q10:Q14").Select(c => c.GetValue<string>()).Where(t => !string.IsNullOrEmpty(t)).Select(t => double.Parse(t)).ToArray();
        Dictionary<string, double> thicknessesByMaterial = [];
        for (int i = 0; i < materials.Length; i++) {
            thicknessesByMaterial[materials[i]] = thicknesses[i];
        }

        var doorTypes = sheet.Cells("Door_Types").Select(c => c.GetValue<string>()).ToArray();
        var doorPanelCounts = sheet.Cells("Door_Types_Panel_Counts").Select(c => (int) c.GetDouble()).ToArray();
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

}
