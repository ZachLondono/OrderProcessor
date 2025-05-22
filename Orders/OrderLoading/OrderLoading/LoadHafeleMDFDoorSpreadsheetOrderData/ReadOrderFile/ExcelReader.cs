using ClosedXML.Excel;

namespace OrderLoading.LoadHafeleMDFDoorSpreadsheetOrderData.ReadOrderFile;

public class ExcelReader {

    public DateTime Date { get; set; }
    public string ProductionTime { get; set; } = string.Empty;
    public string PurchaseOrder { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;

    public string HafelePO { get; set; } = string.Empty;
    public string HafeleOrderNumber { get; set; } = string.Empty;

    public string OrderComments { get; set; } = string.Empty;

    public string Material { get; set; } = string.Empty;
    public string DoorStyle { get; set; } = string.Empty;
    public double Rails { get; set; }
    public double Stiles { get; set; }
    public double AStyleDrawerFrontRails { get; set; }
    public string EdgeProfile { get; set; } = string.Empty;
    public string PanelDetail { get; set; } = string.Empty;
    public double PanelDrop { get; set; }
    public string Finish { get; set; } = string.Empty;
    public string HingeDrilling { get; set; } = string.Empty;
    public double HingeTab { get; set; }

    public string Contact { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Delivery { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;

    public static ParsingResult<Options> LoadFromWorkbook(XLWorkbook workbook) {

        var monad = new Monad(workbook)
                        .ReadDateTimeValue("Ordered_Date", (v, r) => r.Date = v, false)
                        .ReadStringValue("Production_Time", (v, r) => r.ProductionTime = v)
                        .ReadStringValue("Customer_PO", (v, r) => r.PurchaseOrder = v)
                        .ReadStringValue("Customer_Job_Name", (v, r) => r.JobName = v)
                        .ReadStringValue("Hafele_PO", (v, r) => r.HafelePO = v)
                        .ReadStringValue("Hafele_Order_Number", (v, r) => r.HafeleOrderNumber = v, false)
                        .ReadStringValue("Order_Comments", (v, r) => r.OrderComments = v, false)
                        .ReadStringValue("Material", (v, r) => r.Material = v)
                        .ReadStringValue("Framing_Bead", (v, r) => r.DoorStyle = v)
                        .ReadDoubleValue("Default_Rails", (v, r) => r.Rails = v)
                        .ReadDoubleValue("Default_Stiles", (v, r) => r.Stiles = v)
                        .ReadDoubleValue("Default_A_Rails", (v, r) => r.AStyleDrawerFrontRails = v)
                        .ReadStringValue("Edge_Profile", (v, r) => r.EdgeProfile = v)
                        .ReadStringValue("Panel_Detail", (v, r) => r.PanelDetail = v)
                        .ReadDoubleValue("Panel_Drop", (v, r) => r.PanelDrop = v)
                        .ReadStringValue("Finish_Type", (v, r) => r.Finish = v)
                        .ReadStringValue("Hinge_Drilling", (v, r) => r.HingeDrilling = v)
                        .ReadDoubleValue("Hinge_Tab", (v, r) => r.HingeTab = v)
                        .ReadStringValue("Contact", (v, r) => r.Contact = v, false)
                        .ReadStringValue("Company", (v, r) => r.Company = v, false)
                        .ReadStringValue("Account_Number", (v, r) => r.AccountNumber = v, false)
                        .ReadStringValue("Address_1", (v, r) => r.AddressLine1 = v, false)
                        .ReadStringValue("Address_2", (v, r) => r.AddressLine2 = v, false)
                        .ReadStringValue("City", (v, r) => r.City = v, false)
                        .ReadStringValue("State", (v, r) => r.State = v, false)
                        .ReadStringValue("Zip", (v, r) => r.Zip = v, false)
                        .ReadStringValue("Phone", (v, r) => r.Phone = v, false)
                        .ReadStringValue("Email", (v, r) => r.Email = v, false)
                        .ReadStringValue("Delivery_Selection", (v, r) => r.Delivery = v, false)
                        .ReadStringValue("Tracking_Number", (v, r) => r.TrackingNumber = v, false);

        var errors = monad.Errors; 
        var warnings = monad.Warnings;
        var options = monad.ExcelReader.MapToOptions();
        return new ParsingResult<Options>(warnings, errors, options);

    }

    private Options MapToOptions()
        => new() {
            Date = Date,
            ProductionTime = ProductionTime,
            PurchaseOrder = PurchaseOrder,
            JobName = JobName,
            HafelePO = HafelePO,
            HafeleOrderNumber = HafeleOrderNumber,
            OrderComments = OrderComments,
            Material = Material,
            DoorStyle = DoorStyle,
            Rails = Rails,
            Stiles = Stiles,
            AStyleDrawerFrontRails = AStyleDrawerFrontRails,
            EdgeProfile = EdgeProfile,
            PanelDetail = PanelDetail,
            PanelDrop = PanelDrop,
            Finish = Finish,
            HingeDrilling = HingeDrilling,
            HingeTab = HingeTab,
            Contact = Contact,
            Company = Company,
            AccountNumber = AccountNumber,
            AddressLine1 = AddressLine1,
            AddressLine2 = AddressLine2,
            City = City,
            State = State,
            Zip = Zip,
            Phone = Phone,
            Email = Email,
            Delivery = Delivery,
            TrackingNumber = TrackingNumber
        };

    private class Monad {

        public ExcelReader ExcelReader { get; } = new();

        private List<string> _errors = [];
        public IEnumerable<string> Errors => _errors;

        private List<string> _warnings = [];
        public IEnumerable<string> Warnings => _warnings;

        private readonly IXLWorkbook _workbook;
        private IXLWorksheet? _worksheet = null;

        public Monad(IXLWorkbook workbook) {
            _workbook = workbook;
        }

        private IXLWorksheet? GetWorksheet() {

            if (_worksheet is null) {

                try {
                    _worksheet = _workbook.Worksheet("Options");
                } catch {
                    _errors.Add("Options worksheet not found");
                    return null;
                }

            }

            return _worksheet;

        }

        public Monad WorksheetAction(Action<IXLWorksheet, ExcelReader> action, string errorMessage, bool isRequired = true) {

            try {

                var sheet = GetWorksheet();
                if (sheet is not null) {
                    action(sheet, ExcelReader);
                }

            } catch {

                if (isRequired) {
                    _errors.Add(errorMessage);
                } else {
                    _warnings.Add(errorMessage);
                }

            }

            return this;

        }

        public Monad ReadStringValue(string range, Action<string, ExcelReader> action, bool isRequired = true) {

            try {

                var sheet = GetWorksheet();
                if (sheet is not null) {

                    var value = sheet.Cell(range).GetValue<string>();

                    action(value, ExcelReader);

                }

            } catch {

                string msg = $"Could not read value from range '{range}' in workbook.";
                if (isRequired) {
                    _errors.Add(msg);
                } else {
                    _warnings.Add(msg);
                }

            }

            return this;

        }

        public Monad ReadDoubleValue(string range, Action<double, ExcelReader> action, bool isRequired = true) {

            try {

                var sheet = GetWorksheet();
                if (sheet is not null) {

                    var value = sheet.Cell(range).GetDouble();

                    action(value, ExcelReader);

                }

            } catch {

                string msg = $"Could not read value from range '{range}' in workbook.";
                if (isRequired) {
                    _errors.Add(msg);
                } else {
                    _warnings.Add(msg);
                }

            }

            return this;

        }

        public Monad ReadDateTimeValue(string range, Action<DateTime, ExcelReader> action, bool isRequired = true) {

            try {

                var sheet = GetWorksheet();
                if (sheet is not null) {

                    DateTime value;

                    var valueStr = sheet.Cell(range).GetString();
                    if (DateParser.TryParseDate(valueStr, out DateTime date)) {

                        value = date;

                    } else {

                        value = sheet.Cell(range).GetDateTime();

                    }

                    action(value, ExcelReader);

                }

            } catch {

                string msg = $"Could not read date value from range '{range}' in workbook.";
                if (isRequired) {
                    _errors.Add(msg);
                } else {
                    _warnings.Add(msg);
                }

            }

            return this;

        }

    }

}
