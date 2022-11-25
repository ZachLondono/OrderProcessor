using ApplicationCore.Features.Companies.Commands;
using ApplicationCore.Features.Companies.Domain;
using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Features.Orders.Domain.ValueObjects;
using ApplicationCore.Features.Orders.Loader.Providers.DTO;
using ApplicationCore.Features.Orders.Loader.Providers.Results;
using ApplicationCore.Infrastructure;
using ApplicationCore.Shared;
using ClosedXML.Excel;

namespace ApplicationCore.Features.Orders.Loader.Providers;

internal class HafeleExcelProvider : OrderProvider {

	private readonly IBus _bus;
    private readonly LoadingMessagePublisher _publisher;
    private readonly IFileReader _fileReader;
	private readonly HafeleConfiguration _configuration;

	public HafeleExcelProvider(IBus bus, LoadingMessagePublisher publisher, IFileReader fileReader, HafeleConfiguration configuration) {
		_bus = bus;
        _publisher = publisher;
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
            _publisher.PublishError($"Could not find order sheet in workbook");
            return null;
        }

		var orderDateStr = sheet.Cell("OrderDate").ReadString();
        if (!DateTime.TryParse(orderDateStr, out DateTime orderDate)) {
			orderDate = DateTime.Today;
            _publisher.PublishWarning($"Could not read order date '{orderDateStr}'");
        }

		Company? customer = await GetCustomerFromWorksheet(sheet);
		if (customer is null) {
            _publisher.PublishError("Could not read/save customer information");
            return null;
		}


        var unitStr = sheet.Cell("Notation").ReadString();
		Units units = Units.Inches;
		switch (unitStr) {
			case "Metric":
				units = Units.Millimeters;
				break;
			case "Fraction":
			case "Decimal":
				units = Units.Inches;
				break;
			default:
				_publisher.PublishWarning($"Unrecognized unit selection '{unitStr}'");
				break;
		}

		var freight = wb.Cell("Standard_Freight").ReadDecimal();
		var globalOptions = GetGlobalOptionsFromSheet(sheet);

		var logoOption = sheet.Cell("LogoOption").ReadString().ToLower();
		string deliveryMethod = sheet.Cell("DeliverySelection").ReadString();
		var items = GetAdditionalItems(wb, logoOption, deliveryMethod);

		var productionTime = sheet.Cell("ProductionSelection").ReadString();
		bool rush = false;
		switch (productionTime) {
			case "Standard - 1 Week":
				rush = false;
				break;
			case "3 Day Rush":
				rush = true;
				break;
			default:
                _publisher.PublishWarning($"Unrecognized production time selection '{productionTime}'");
				break;
		}

		string customerPO = sheet.Cell("K7").ReadString();
		string jobName = sheet.Cell("jobname").ReadString();
		string hafelePO = sheet.Cell("K11").ReadString();
		string hafeleOrderNum = sheet.Cell("K12").ReadString();
		string contact = sheet.Cell("V3").ReadString();
        string accntNum = sheet.Cell("V5").ReadString();

        var info = new Dictionary<string, string>() {
			{ "Customer PO", customerPO },
			{ "Hafele Order Number", hafeleOrderNum },
			{ "Delivery", deliveryMethod },
			{ "Contact", contact },
			{ "Account Number", accntNum }
		};

		var comment = sheet.Cell("N12").ReadString();

		var data = new DataColumns() {
			Line = sheet.Cell("A17"),
			Qty = sheet.Cell("B17"),
			Height = sheet.Cell("F17"),
			Width = sheet.Cell("G17"),
			Depth = sheet.Cell("H17"),
			Pullout = sheet.Cell("I17"),
			Bottom = sheet.Cell("J17"),
			Notch = sheet.Cell("K17"),
			Logo = sheet.Cell("L17"),
			Clips = sheet.Cell("M17"),
			Accessory = sheet.Cell("N17"),
			JobName = sheet.Cell("O17"),
            UnitPrice = sheet.Cell("P17"),
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

			var qtyCell = data.Qty.GetOffsetCell(rowOffset: offset);
			if (qtyCell.ValueIsNullOrWhitespace()) {
				offset++;
				continue;
			}

			var wasRead = TryReadBox(data, offset, out DrawerBoxData box, units, globalOptions);

			if (wasRead) {

				boxes.Add(box);

			} else {
                _publisher.PublishWarning($"Error reading drawer box on row '{qtyCell.WorksheetRow().RowNumber()}'");
            }

			offset++;

		}

		return new OrderData() {
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
			AdditionalItems = items,
			Rush = rush
		};

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

	private List<AdditionalItemData> GetAdditionalItems(XLWorkbook wb, string logoOption, string deliveryMethod) {
		var items = new List<AdditionalItemData>();
		if (logoOption.Equals("Yes-Inside w/ Setup") || logoOption.Equals("Yes-Outside w/ Setup")) {

			var setupFee = wb.Cell("SetupFee").ReadDecimal();

			items.Add(new() {
				Description = "Logo Setup",
				Price = setupFee
			});

		} else if (logoOption.Equals("Yes-Inside") && logoOption.Equals("Yes-Outside") && logoOption.Equals("No") && !string.IsNullOrWhiteSpace(logoOption)) {
			_publisher.PublishWarning($"Unknown logo selection '{logoOption}'");
        }

		if (deliveryMethod.Equals("Liftgate required")) {

			// Read liftgate price from workbook
			var liftgateFee = 25M;

			items.Add(new() {
				Description = "Liftgate Delivery",
				Price = liftgateFee
			});

		} else if (!deliveryMethod.Equals("Standard Pallet") && !string.IsNullOrWhiteSpace(deliveryMethod)) {
			_publisher.PublishWarning($"Unknown delivery method selection '{deliveryMethod}'");
		}

		return items;
	}

	private GlobalOptions GetGlobalOptionsFromSheet(IXLWorksheet sheet) {
        string materialName = sheet.Cell("Material").ReadString();
        string assembledStr = sheet.Cell("Assembled").ReadString();
        string postFinishStr = sheet.Cell("PostFinish").ReadString();
        string mountingHolesStr = sheet.Cell("MountingHoles").ReadString();

        var globalOptions = new GlobalOptions() {
            MaterialId = GetMaterialId(materialName),
            Assembled = ParseAssemblyOption(assembledStr),
            PostFinished = ParsePostFinishOption(postFinishStr),
            FaceMountingHoles = ParseMountHolesOption(mountingHolesStr)
        };

        return globalOptions;
    }

	private bool ParseMountHolesOption(string mountingHolesStr) {
        switch (mountingHolesStr) {
            case "Yes":
                return true;
            case "":
            case "No":
                return false;
            default:
                _publisher.PublishWarning($"Unknown mounting holes option option '{mountingHolesStr}'");
                return false;
        }
    }

    private bool ParsePostFinishOption(string postFinishStr) {
        switch (postFinishStr) {
            case "Yes":
                return true;
            case "":
            case "No":
                return false;
            default:
                _publisher.PublishWarning($"Unknown post finish option option '{postFinishStr}'");
                return false;
        }
    }

    private bool ParseAssemblyOption(string assembledStr) {
        switch (assembledStr) {
            case "":
            case "Assembled":
                return true;
            case "FlatPack":
                return false;
            default:
                _publisher.PublishWarning($"Unknown assebly option '{assembledStr}'");
				return true;
        }
    }

    private async Task<Company?> GetCustomerFromWorksheet(IXLWorksheet sheet) {

		// TODO: move reading from sheet outside of function

		string accntNum = sheet.Cell("V5").ReadString();
        
		Company? customer = null;
        bool hasError = false;
        
		if (string.IsNullOrWhiteSpace(accntNum)) {
			_publisher.PublishWarning($"No customer account number found");
        } else {

            var customerResponse = await _bus.Send(new GetCompanyByHafeleAccountNumber.Query(accntNum));

            customerResponse.Match(
                c => {
                    customer = c;
                },
                error => {
                    hasError = true;
                    _publisher.PublishError(error.Title);
                }
            );

            if (hasError) return null;

        }

        if (customer is null) {

            string company = sheet.Cell("V4").ReadString();

			if (string.IsNullOrWhiteSpace(company)) {
                _publisher.PublishWarning("No customer name entered");
				return null;
            }

            string contact = sheet.Cell("V3").ReadString();
			string addr1 = sheet.Cell("V6").ReadString();
			string addr2 = sheet.Cell("V7").ReadString();
			string city = sheet.Cell("V8").ReadString();
			string state = sheet.Cell("V9").ReadString();
			string zip = sheet.Cell("V10").ReadString();
			string phone = sheet.Cell("V11").ReadString();
			string email = sheet.Cell("V12").ReadString();

			var address = new Companies.Domain.ValueObjects.Address() {
				Line1 = addr1,
				Line2 = addr2,
				Line3 = string.Empty,
				City = city,
				State = state,
				Zip = zip,
				Country = "USA"
			};

			var createResponse = await _bus.Send(new CreateCompany.Command(company, address, phone, email, email, contact));

			createResponse.Match(
				c => {
					customer = c;
				},
				error => {
					hasError = true;
                    _publisher.PublishError(error.Title);
                }
            );

			if (customer is not null) {

				await _bus.Send(new CreateHafeleIdCompanyIdMapping.Command(accntNum, customer.Id));

			}

		}

		if (hasError || customer is null) return null;

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
			
			var height = data.Height.GetOffsetCell(offset).ReadDouble();
			var width = data.Width.GetOffsetCell(offset).ReadDouble();
			var depth = data.Depth.GetOffsetCell(offset).ReadDouble();
			if (units == Units.Millimeters) {
				box.Height = Dimension.FromMillimeters(height);
				box.Width = Dimension.FromMillimeters(width);
				box.Depth = Dimension.FromMillimeters(depth);
			} else {
				box.Height = Dimension.FromInches(height);
				box.Width = Dimension.FromInches(width);
				box.Depth = Dimension.FromInches(depth);
			}

			box.Line = data.Line.GetOffsetCell(offset).ReadInt();
			box.Qty = data.Qty.GetOffsetCell(offset).ReadInt();
			box.ScoopFront = data.Pullout.GetOffsetCell(offset).ReadString().Equals("Scoop Front");
			var bottom = data.Bottom.GetOffsetCell(offset).ReadString();
			box.BottomMaterialOptionId = GetMaterialId(bottom);
			box.Notch = data.Notch.GetOffsetCell(offset).ReadString();
			box.Clips = data.Clips.GetOffsetCell(offset).ReadString();
			box.Accessory = data.Accessory.GetOffsetCell(offset).ReadString();
			var jobName = data.JobName.GetOffsetCell(offset).ReadString();
			box.UnitPrice = data.UnitPrice.GetOffsetCell(offset).ReadDecimal();
			box.LabelFields.Add("Job Name", jobName);
			box.Note = data.Note.GetOffsetCell(offset).ReadString();
			box.Logo = data.Logo.GetOffsetCell(offset).ReadString() switch {
				"Yes" => LogoPosition.Outside,
				_ => LogoPosition.None
			};

			if (box.Accessory.Equals("Cubes")) {

				box.FixedDividers = true;
				box.DividersDeep = data.CubeFB.GetOffsetCell(offset).ReadInt();
				box.DividersWide = data.CubeLR.GetOffsetCell(offset).ReadInt();

			} else if (box.Accessory.Equals("U-Box")) {

				box.UBox = true;
				var a = data.UBoxA.GetOffsetCell(offset).ReadDouble();
				var b = data.UBoxA.GetOffsetCell(offset).ReadDouble();
				var c = data.UBoxA.GetOffsetCell(offset).ReadDouble();

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

		} catch (Exception ex) {
			_publisher.PublishWarning(ex.Message);
            return false;
		} 

	}

	private Guid GetMaterialId(string optionname) {
		if (_configuration.MaterialMap.TryGetValue(optionname, out string? optionidstr) && optionidstr is not null) {
			var optionid = Guid.Parse(optionidstr);
			return optionid;
		}
        _publisher.PublishWarning($"Unknown or empty material selection '{optionname}'");
        return Guid.Empty;
	}

	struct DataColumns {
		public IXLCell Line { get; set; }
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
        public IXLCell UnitPrice { get; set; }
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
