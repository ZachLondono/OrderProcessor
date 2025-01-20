using ClosedXML.Excel;

namespace ApplicationCore.Features.HafeleMDFDoorOrders.ReadOrderFile;

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
    public required string Rails { get; init; }
    public required string Stiles { get; init; }
    public required string AStyleDrawerFrontRails { get; init; }
    public required string EdgeProfile { get; init; }
    public required string PanelDetail { get; init; }
    public required string PanelDrop { get; init; }
    public required string Finish { get; init; }
    public required string HingeDrilling { get; init; }
    public required string HingeTab { get; init; }

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

    public static Options LoadFromWorkbook(XLWorkbook workbook) {

        var sheet = workbook.Worksheet("Options");

        var data = sheet.Range("Ordered_Date").FirstCell().Value;

        return new() {
            Date = DateTime.UnixEpoch, //sheet.Cell("Date").GetValue<DateTime>(),
            ProductionTime = sheet.Cell("Production_Time").GetValue<string>(),
            PurchaseOrder = sheet.Cell("Customer_PO").GetValue<string>(),
            JobName = sheet.Cell("Customer_Job_Name").GetValue<string>(),
            HafelePO = sheet.Cell("Hafele_PO").GetValue<string>(),
            HafeleOrderNumber = sheet.Cell("Hafele_Order_Number").GetValue<string>(),
            OrderComments = sheet.Cell("Order_Comments").GetValue<string>(),
            Material = sheet.Cell("Material").GetValue<string>(),
            DoorStyle = sheet.Cell("Framing_Bead").GetValue<string>(),
            Rails = sheet.Cell("Default_Rails").GetValue<string>(),
            Stiles = sheet.Cell("Default_Stiles").GetValue<string>(),
            AStyleDrawerFrontRails = sheet.Cell("Default_A_Rails").GetValue<string>(),
            EdgeProfile = sheet.Cell("Edge_Profile").GetValue<string>(),
            PanelDetail = sheet.Cell("Panel_Detail").GetValue<string>(),
            PanelDrop = sheet.Cell("Panel_Drop").GetValue<string>(),
            Finish = sheet.Cell("Finish_Type").GetValue<string>(),
            HingeDrilling = sheet.Cell("Hinge_Drilling").GetValue<string>(),
            HingeTab = sheet.Cell("Hinge_Tab").GetValue<string>(),
            Contact = sheet.Cell("Contact").GetValue<string>(),
            Company = sheet.Cell("Company").GetValue<string>(),
            AccountNumber = sheet.Cell("Account_Number").GetValue<string>(),
            AddressLine1 = sheet.Cell("Address_1").GetValue<string>(),
            AddressLine2 = sheet.Cell("Address_2").GetValue<string>(),
            City = sheet.Cell("City").GetValue<string>(),
            State = sheet.Cell("State").GetValue<string>(),
            Zip = sheet.Cell("Zip").GetValue<string>(),
            Phone = sheet.Cell("Phone").GetValue<string>(),
            Email = sheet.Cell("Email").GetValue<string>(),
            Delivery = sheet.Cell("Delivery_Selection").GetValue<string>()
        };

    }

}
