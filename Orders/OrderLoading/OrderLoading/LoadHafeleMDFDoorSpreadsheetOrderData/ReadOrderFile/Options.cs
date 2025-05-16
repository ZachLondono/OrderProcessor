using ClosedXML.Excel;

namespace OrderLoading.LoadHafeleMDFDoorSpreadsheetOrderData.ReadOrderFile;

public class Options {

    public required DateTime Date { get; init; }
    public required string ProductionTime { get; init; }
    public required string PurchaseOrder { get; init; }
    public required string JobName { get; init; }

    public required string HafelePO { get; init; }
    public required string HafeleOrderNumber { get; init; }

    public required string OrderComments { get; init; }

    public required string Material { get; init; }
    public required string DoorStyle { get; init; }
    public required double Rails { get; init; }
    public required double Stiles { get; init; }
    public required double AStyleDrawerFrontRails { get; init; }
    public required string EdgeProfile { get; init; }
    public required string PanelDetail { get; init; }
    public required double PanelDrop { get; init; }
    public required string Finish { get; init; }
    public required string HingeDrilling { get; init; }
    public required double HingeTab { get; init; }

    public required string Contact { get; init; }
    public required string Company { get; init; }
    public required string AccountNumber { get; init; }
    public required string AddressLine1 { get; init; }
    public required string AddressLine2 { get; init; }
    public required string City { get; init; }
    public required string State { get; init; }
    public required string Zip { get; init; }
    public required string Phone { get; init; }
    public required string Email { get; init; }
    public required string Delivery { get; init; }
    public required string TrackingNumber { get; init; }

    public static ParsingResult<Options> LoadFromWorkbook(XLWorkbook workbook) {
        return ExcelReader.LoadFromWorkbook(workbook);
    }

    public DateTime GetDueDate() => Date.AddDays(GetLeadTime(ProductionTime));

    private static int GetLeadTime(string leadTime) => leadTime switch {
        "Standard 10 day" => 10,
        "5 Day Rush" => 5,
        _ => throw new InvalidOperationException($"Unexpected lead time - '{leadTime}'")
    };

}
