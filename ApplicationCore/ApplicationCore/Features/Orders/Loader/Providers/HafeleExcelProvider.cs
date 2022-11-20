using ApplicationCore.Features.Companies.Commands;
using ApplicationCore.Features.Companies.Domain;
using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Loader.Providers.DTO;
using ApplicationCore.Infrastructure;
using ApplicationCore.Shared;
using ClosedXML.Excel;

namespace ApplicationCore.Features.Orders.Loader.Providers;

internal class HafeleExcelProvider : OrderProvider {

	private readonly IBus _bus;
	private readonly IFileReader _fileReader;
	private readonly HafeleConfiguration _configuration;

	public HafeleExcelProvider(IBus bus, IFileReader fileReader, HafeleConfiguration configuration) {
		_bus = bus;
		_fileReader = fileReader;
		_configuration = configuration;
	}

	public override async Task<OrderData?> LoadOrderData(string source) {

		var vendorId = new Guid(_configuration.VendorId);

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

		Company customer = await GetCustomerFromWorksheet(sheet);

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

		var freight = wb.Cell("Standard_Freight").GetValue<decimal>();
		var globalOptions = GetGlobalOptionsFromSheet(sheet);

		var logoOption = sheet.Cell("LogoOption").GetValue<string>().ToLower();
		string deliveryMethod = sheet.Cell("DeliverySelction").GetValue<string>();
		var items = GetAdditionalItems(wb, logoOption, deliveryMethod);

		string customerPO = sheet.Cell("K7").GetValue<string>();
		string jobName = sheet.Cell("jobname").GetValue<string>();
		string hafelePO = sheet.Cell("K11").GetValue<string>();
		string hafeleOrderNum = sheet.Cell("K12").GetValue<string>();
		string contact = sheet.Cell("V3").GetValue<string>();

		var info = new Dictionary<string, string>() {
			{ "Customer PO", customerPO },
			{ "Hafele Order Number", hafeleOrderNum },
			{ "Delivery", deliveryMethod },
			{ "Contact", contact }
		};

		var comment = sheet.Cell("N12").GetValue<string>();

		var data = new DataColumns() {
			Qty = sheet.Cell("B17"),
			Width = sheet.Cell("F17"),
			Height = sheet.Cell("F17"),
			Depth = sheet.Cell("H17"),
			Pullout = sheet.Cell("I17"),
			Bottom = sheet.Cell("J17"),
			Notch = sheet.Cell("K17"),
			Logo = sheet.Cell("L17"),
			Clips = sheet.Cell("M17"),
			Accessory = sheet.Cell("N17"),
			JobName = sheet.Cell("O17"),
			Note = sheet.Cell("S17"),
			UBoxA = sheet.Cell("U17"),
			UBoxB = sheet.Cell("U17"),
			UBoxC = sheet.Cell("U17"),
			CubeLR = sheet.Cell("Y17"),
			CubeFB = sheet.Cell("Z17")
		};

		var boxes = new List<DrawerBoxData>();

		int offset = 0;
		while (offset < 100) {

			var qtyStr = data.Qty.GetOffsetCell(rowOffset: offset).GetString();
			if (string.IsNullOrEmpty(qtyStr)) {
				offset++;
				continue;
			}

			var wasRead = TryReadBox(data, offset, out DrawerBoxData box, units, globalOptions);

			if (wasRead) {

				boxes.Add(box);

			} else {

				// Warn about error reading box

			}

			offset++;

		}

		var order = new OrderData() {
			Number = hafelePO,
			Name = jobName,
			OrderDate = orderDate,
			Comment = comment,
			CustomerId = customer.Id,
			PriceAdjustment = 0M,
			Shipping = freight,
			Tax = 0,
			VendorId = vendorId,
			Boxes = boxes,
			Info = info,
			AdditionalItems = items
		};

		return order;

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

	private static List<AdditionalItemData> GetAdditionalItems(XLWorkbook wb, string logoOption, string deliveryMethod) {
		var items = new List<AdditionalItemData>();
		if (logoOption.Contains("setup")) {

			var setupFee = wb.Cell("SetupFee").GetValue<decimal>();

			items.Add(new() {
				Description = "Logo Setup",
				Price = setupFee
			});

		}

		if (deliveryMethod.Equals("Liftgate required")) {

			// Read liftgate price from workbook
			var liftgateFee = 25M;

			items.Add(new() {
				Description = "Liftgate Delivery",
				Price = liftgateFee
			});

		}

		return items;
	}

	private GlobalOptions GetGlobalOptionsFromSheet(IXLWorksheet sheet) {
		string materialName = sheet.Cell("Material").GetValue<string>();
		string assembledStr = sheet.Cell("Assembled").GetValue<string>();
		string postFinishStr = sheet.Cell("PostFinish").GetValue<string>();
		string mountingHolesStr = sheet.Cell("MountingHoles").GetValue<string>();

		var globalOptions = new GlobalOptions() {

			MaterialId = GetMaterialId(materialName),

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

		return globalOptions;
	}

	private async Task<Company> GetCustomerFromWorksheet(IXLWorksheet sheet) {

		// TODO: move reading from sheet outside of function

		string accntNum = sheet.Cell("V5").GetValue<string>();
		var customerResponse = await _bus.Send(new GetCompanyByHafeleAccountNumber.Query(accntNum));

		Company? customer = null;
		bool hasError = false;
		customerResponse.Match(
			c => {
				customer = c;
			},
			error => {
				hasError = true;
				// TODO: log error
			}
		);

		if (hasError) {
			throw new InvalidDataException("Could not get customer");
		}

		if (customer is null) {

			string company = sheet.Cell("V4").GetValue<string>();
			string addr1 = sheet.Cell("V6").GetValue<string>();
			string addr2 = sheet.Cell("V7").GetValue<string>();
			string city = sheet.Cell("V8").GetValue<string>();
			string state = sheet.Cell("V9").GetValue<string>();
			string zip = sheet.Cell("V10").GetValue<string>();
			string phone = sheet.Cell("V11").GetValue<string>();
			string email = sheet.Cell("V12").GetValue<string>();

			var address = new Companies.Domain.ValueObjects.Address() {
				Line1 = addr1,
				Line2 = addr2,
				Line3 = string.Empty,
				City = city,
				State = state,
				Zip = zip,
				Country = "USA"
			};

			var createResponse = await _bus.Send(new CreateCompany.Command(company, address, phone, email, email));

			createResponse.Match(
				c => {
					customer = c;
				},
				error => {
					hasError = true;
					// TODO: log error
				}
			);

		}

		if (hasError || customer is null) {
			throw new InvalidDataException("Could not get customer");
		}

		return customer;

	}

	private bool TryReadBox(DataColumns data, int offset, out DrawerBoxData box, Units units, GlobalOptions options) {

		box = new DrawerBoxData {
			BoxMaterialOptionId = options.MaterialId,
			Assembled = options.Assembled,
			PostFinish = options.PostFinished,
			FaceMountingHoles = options.FaceMountingHoles
		};

		try {

			// TODO: formulas may make the values unreadable, may need to read cached result
			
			var height = data.Height.GetOffsetCell(offset).GetValue<double>();
			var width = data.Width.GetOffsetCell(offset).GetValue<double>();
			var depth = data.Depth.GetOffsetCell(offset).GetValue<double>();
			if (units == Units.Millimeters) {
				box.Height = Dimension.FromMillimeters(height);
				box.Width = Dimension.FromMillimeters(width);
				box.Depth = Dimension.FromMillimeters(depth);
			} else {
				box.Height = Dimension.FromInches(height);
				box.Width = Dimension.FromInches(width);
				box.Depth = Dimension.FromInches(depth);
			}

			box.Qty = data.Qty.GetOffsetCell(offset).GetValue<int>();
			box.ScoopFront = data.Pullout.GetOffsetCell(offset).GetValue<string>().Equals("Scoop Front");
			var bottom = data.Bottom.GetOffsetCell(offset).GetValue<string>();
			box.BottomMaterialOptionId = GetMaterialId(bottom);
			box.Notch = data.Notch.GetOffsetCell(offset).GetValue<string>();
			box.Logo = data.Logo.GetOffsetCell(offset).GetValue<string>().Equals("Yes");
			box.Clips = data.Clips.GetOffsetCell(offset).GetValue<string>();
			box.Accessory = data.Accessory.GetOffsetCell(offset).GetValue<string>();
			var jobName = data.JobName.GetOffsetCell(offset).GetValue<string>();
			//box.LabelFields.Add("Job Name", jobName);
			box.Note = data.Note.GetOffsetCell(offset).GetValue<string>();

			if (box.Accessory.Equals("Cubes")) {

				box.FixedDividers = true;
				box.DividersDeep = data.CubeFB.GetOffsetCell(offset).GetValue<int>();
				box.DividersWide = data.CubeLR.GetOffsetCell(offset).GetValue<int>();

			} else if(box.Accessory.Equals("UBox")) {

				box.UBox = true;
				var a = data.UBoxA.GetOffsetCell(offset).GetValue<double>();
				var b = data.UBoxA.GetOffsetCell(offset).GetValue<double>();
				var c = data.UBoxA.GetOffsetCell(offset).GetValue<double>();

				if (units == Units.Millimeters) {
					box.UBoxA = Dimension.FromMillimeters(a);
					box.UBoxB = Dimension.FromMillimeters(b);
					box.UBoxC = Dimension.FromMillimeters(c);
				} else {
					box.UBoxA = Dimension.FromInches(a);
					box.UBoxB = Dimension.FromInches(b);
					box.UBoxC = Dimension.FromInches(c);
				}

			}

			return true;

		} catch {
			// TODO: log exception
			return false;
		} 

	}

	private Guid GetMaterialId(string optionname) {
		if (_configuration.MaterialMap.TryGetValue(optionname, out string? optionidstr) && optionidstr is not null) {
			var optionid = Guid.Parse(optionidstr);
			return optionid;
		}
		return Guid.Parse("d3030d0a-8992-4b6b-8577-9d4ac43b7cf7");
	}

	struct DataColumns {
		public IXLCell Qty {  get; set; }
		public IXLCell Width { get; set; }
		public IXLCell Height { get; set; }
		public IXLCell Depth { get; set; }
		public IXLCell Pullout { get; set; }
		public IXLCell Bottom { get; set; }
		public IXLCell Notch { get; set; }
		public IXLCell Logo { get; set; }
		public IXLCell Clips { get; set; }
		public IXLCell Accessory { get; set; }
		public IXLCell JobName { get; set; }
		public IXLCell Note { get; set; }
		public IXLCell UBoxA { get; set; }
		public IXLCell UBoxB { get; set; }
		public IXLCell UBoxC { get; set; }
		public IXLCell CubeLR { get; set; }
		public IXLCell CubeFB { get; set; }
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
