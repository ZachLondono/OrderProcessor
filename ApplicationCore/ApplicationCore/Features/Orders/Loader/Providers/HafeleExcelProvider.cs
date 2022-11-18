using ApplicationCore.Features.Orders.Loader.Providers.DTO;
using ApplicationCore.Shared;
using ClosedXML.Excel;

namespace ApplicationCore.Features.Orders.Loader.Providers;

internal class HafeleExcelProvider : OrderProvider {

	private readonly IFileReader _fileReader;

	public HafeleExcelProvider(IFileReader fileReader) {
		_fileReader = fileReader;
	}

	public override Task<OrderData?> LoadOrderData(string source) {

		using var stream = _fileReader.OpenReadFileStream(source);
		using var wb = new XLWorkbook(stream);

		IXLWorksheet sheet;
		if (wb.Worksheets.Contains("Order Sheet")) {
			sheet = wb.Worksheet("Order Sheet");
		} else if (wb.Worksheets.Contains("Dovetail")) {
			sheet = wb.Worksheet("Dovetail");
		} else {
			throw new InvalidDataException("Workbook does not contain order data worksheet");
		}

		if (!DateTime.TryParse(sheet.Cell("OrderDate").GetValue<string>(), out DateTime orderDate)) {
			orderDate = DateTime.Today;
		}

		string customerPO = sheet.Cell("K7").GetValue<string>();
		string jobName = sheet.Cell("jobname").GetValue<string>();
		string hafelePO = sheet.Cell("K11").GetValue<string>();
		string hafeleOrderNum = sheet.Cell("K12").GetValue<string>();

		string contact = sheet.Cell("V3").GetValue<string>();
		string company = sheet.Cell("V4").GetValue<string>();
		string accntNum = sheet.Cell("V5").GetValue<string>();
		string addr1 = sheet.Cell("V6").GetValue<string>();
		string addr2 = sheet.Cell("V7").GetValue<string>();
		string city = sheet.Cell("V8").GetValue<string>();
		string state = sheet.Cell("V9").GetValue<string>();
		string zip = sheet.Cell("V10").GetValue<string>();
		string phone = sheet.Cell("V11").GetValue<string>();
		string email = sheet.Cell("V12").GetValue<string>();
		string deliveryMethod = sheet.Cell("DeliverySelction").GetValue<string>();

		string materialName = sheet.Cell("Material").GetValue<string>();
		string assembledStr = sheet.Cell("Assembled").GetValue<string>();
		string postFinishStr = sheet.Cell("PostFinish").GetValue<string>();
		string mountingHolesStr = sheet.Cell("MountingHoles").GetValue<string>();

		var unitStr = sheet.Cell("Notation").GetValue<string>();
		var units = unitStr switch {
			"Metric" => Units.Millimeters,
			"Fraction" => Units.Inches,
			"Decimal" => Units.Inches,
			_ => Units.Inches, // TODO: show a warning when an unrecognized value is found
		};

		var productionTime = sheet.Cell("ProductionSelection").GetValue<string>();
		bool rush = productionTime switch {
			"Standard - 1 Week" => false,
			"3 Day Rush" => true,
			_ => false, // TODO: show a warning when an unrecognized value is found
		};

		var globalOptions = new GlobalOptions() {

			MaterialId = Guid.Empty, // TODO: get material id
			
			Assembled = assembledStr switch {
				"Assembled" or "" => true,
				"Flat Pack" => false,
				_ => true // TODO: show a warning when an unrecognized value is found
			},
			
			PostFinished = postFinishStr switch {
				"Yes" => true,
				"No" or "" => false,
				_ => false // TODO: show a warning when an unrecognized value is found
			},

			FaceMountingHoles = mountingHolesStr switch {
				"Yes" => true,
				"No" or "" => false,
				_ => false // TODO: show a warning when an unrecognized value is found
			}

		};

		var info = new Dictionary<string, string>() {
			{ "Customer PO", customerPO },
			{ "Hafele Order Number", hafeleOrderNum },
			{ "Delivery", deliveryMethod },
			{ "Contact", contact }
		};

		var items = new List<AdditionalItemData>();
		var logoOption = sheet.Cell("LogoOption").GetValue<string>().ToLower();
		if (logoOption.Contains("setup")) {

			var setupFee = wb.Cell("SetupFee").GetValue<decimal>();

			items.Add(new() {
				Description = "Logo Setup",
				Price = setupFee
			});

		}

		var order = new OrderData() {
			Number = hafelePO,
			Name = jobName,
			OrderDate = orderDate,

			Info = info,
			AdditionalItems = items
		};

		return Task.FromResult((OrderData?) order);

	}

	public override Task<ValidationResult> ValidateSource(string source) {

		if (!File.Exists(source))
			return Task.FromResult(new ValidationResult() {
				IsValid = false,
				ErrorMessage = $"Given file does not exist\n'{source}'"
			});

		var extension = Path.GetExtension(source);
		if (extension != ".xlsx" && extension != ".xlsm")
			return Task.FromResult(new ValidationResult() {
				IsValid = false,
				ErrorMessage = $"Invalid file type '{extension}'"
			});

		return Task.FromResult(new ValidationResult() { IsValid = true });

	}

	struct GlobalOptions {
		public Guid MaterialId { get; set; }
		public bool Assembled { get; set; }
		public bool PostFinished { get; set; }
		public bool FaceMountingHoles { get; set; }
	}

	enum Units {
		Millimeters,
		Inches
	}

}
