using OrderLoading.ClosetProCSVCutList;
using OrderLoading.ClosetProCSVCutList.CSVModels;
using OrderLoading.ClosetProCSVCutList.Products;
using Domain.Companies.ValueObjects;
using Domain.Orders.Builders;
using Domain.Orders.Entities;
using Domain.ValueObjects;
using Dapper;
using Microsoft.Extensions.Logging;
using static Domain.Companies.CompanyDirectory;
using CompanyCustomer = Domain.Companies.Entities.Customer;
using Domain.Orders.Entities.Products;
using Domain.Orders.Persistance;
using Domain.Services;
using OrderLoading.ClosetProCSVCutList.Products.Shelves;
using OrderLoading.ClosetProCSVCutList.Products.Verticals;
using OrderLoading.ClosetProCSVCutList.Products.Fronts;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Hardware;
using Domain.Orders.Entities.Products.Closets;

namespace OrderLoading.LoadClosetProOrderData;

public abstract class ClosetProCSVOrderProvider : IOrderProvider {

	public IOrderLoadWidgetViewModel? OrderLoadingViewModel { get; set; }

	private readonly ILogger<ClosetProCSVOrderProvider> _logger;
	private readonly ClosetProCSVReader _reader;
	private readonly ClosetProPartMapper _partMapper;
	private readonly IFileReader _fileReader;
	private readonly IOrderingDbConnectionFactory _dbConnectionFactory;
	private readonly GetCustomerIdByNameAsync _getCustomerIdByNameAsync;
	private readonly GetCustomerOrderPrefixByIdAsync _getCustomerOrderPrefixByIdAsync;
	private readonly GetCustomerWorkingDirectoryRootByIdAsync _getCustomerWorkingDirectoryRootByIdAsync;
	private readonly InsertCustomerAsync _insertCustomerAsync;
	private readonly GetCustomerByIdAsync _getCustomerByIdAsync;
	private readonly ComponentBuilderFactory _componentBuilderFactory;

	public ClosetProCSVOrderProvider(ILogger<ClosetProCSVOrderProvider> logger, ClosetProCSVReader reader, ClosetProPartMapper partMapper, IFileReader fileReader, IOrderingDbConnectionFactory dbConnectionFactory, GetCustomerIdByNameAsync getCustomerIdByNameIdAsync, InsertCustomerAsync insertCustomerAsync, GetCustomerOrderPrefixByIdAsync getCustomerOrderPrefixByIdAsync, GetCustomerByIdAsync getCustomerByIdAsync, GetCustomerWorkingDirectoryRootByIdAsync getCustomerWorkingDirectoryRootByIdAsync, ComponentBuilderFactory componentBuilderFactory) {
		_logger = logger;
		_reader = reader;
		_partMapper = partMapper;
		_fileReader = fileReader;
		_dbConnectionFactory = dbConnectionFactory;
		_getCustomerIdByNameAsync = getCustomerIdByNameIdAsync;
		_insertCustomerAsync = insertCustomerAsync;
		_getCustomerOrderPrefixByIdAsync = getCustomerOrderPrefixByIdAsync;
		_getCustomerByIdAsync = getCustomerByIdAsync;
		_getCustomerWorkingDirectoryRootByIdAsync = getCustomerWorkingDirectoryRootByIdAsync;
		_componentBuilderFactory = componentBuilderFactory;
	}

	protected abstract Task<string?> GetCSVDataFromSourceAsync(string source);

	public async Task<OrderData?> LoadOrderData(string sourceObj) {

        OrderLoadingSettings settings = GetOrderLoadingSettings(sourceObj);

        var csvData = await GetCSVDataFromSourceAsync(settings.FilePath);

        if (csvData is null) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "No order data found");
            return null;
        }

        _reader.OnReadError += (msg) => OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, msg);
        var info = await _reader.ReadCSVData(csvData);

        // TODO: get this info from a configuration file
        var vendorId = Guid.Parse("a81d759d-5b6c-4053-8cec-55a6c94d609e");
        string designerName = info.Header.GetDesignerName();
        var customer = await GetOrCreateCustomer(info.Header.DesignerCompany, designerName);

        // TODO: need hardware list here
        Dimension hardwareSpread = ClosetProPartMapper.GetHardwareSpread(info.PickList, []);

        var parts = GetProductPartsFromPartList(info.Parts);
        _partMapper.GroupLikeProducts = true; // TODO: Move this into the closet pro settings object
        var cpProducts = _partMapper.MapPartsToProducts(parts, hardwareSpread);
        var products = cpProducts.Select(p => CreateProductFromClosetProProduct(p, customer.ClosetProSettings, _componentBuilderFactory))
                                 .ToList();

        string orderNumber = await GetOrderNumber(settings.CustomOrderNumber, customer);
        string orderName = GetOrderName(info.Header.OrderName);
        string? workingDirectoryRoot = await GetWorkingDirectoryRoot(settings.CustomWorkingDirectoryRoot, customer);
        string workingDirectory = await CreateWorkingDirectory(csvData, info.Header.DesignerCompany, orderName, orderNumber, workingDirectoryRoot);

        (HangingRail[] hangRails, Supply[] hangRailSupplies) = ClosetProPartMapper.GetHangingRailsFromParts(info.Parts);
        (DrawerSlide[] slides, Supply[] slideSupplies) = GetDrawerSlidesFromProducts(products);
        var hangRailBrackets = ClosetProPartMapper.GetHangingRailBracketsFromPickParts(info.PickList).ToArray();

        var supplies = GetSupplies(cpProducts);
        supplies.AddRange(hangRailBrackets);
        supplies.AddRange(hangRailSupplies);
        supplies.AddRange(slideSupplies);

        var suppliesArray = supplies.Where(s => s.Qty != 0).ToArray();
        Hardware hardware = new(suppliesArray, slides, hangRails);

        List<AdditionalItem> additionalItems = [];

        return new OrderData() {
            VendorId = vendorId,
            CustomerId = customer.Id,
            Name = orderName,
            Number = orderNumber,
            WorkingDirectory = workingDirectory,
            Products = products,
            AdditionalItems = additionalItems,

            OrderDate = DateTime.Today,
            DueDate = null,
            Rush = false,
            Info = [],
            Comment = string.Empty,
            PriceAdjustment = 0M,
            Tax = 0M,
            Billing = new() {
                InvoiceEmail = null,
                PhoneNumber = "",
                Address = new()
            },
            Shipping = new() {
                Contact = designerName,
                Address = new(),
                Method = "Pick Up",
                PhoneNumber = "",
                Price = 0M
            },
            Hardware = hardware
        };

    }

	/// <summary>
	/// Returns a list of parts which are products, discarding any which are not
	/// </summary>
    private static IEnumerable<Part> GetProductPartsFromPartList(IEnumerable<Part> parts) {

        string[] productPartTypes = [
            "Material",
            "Drawer",
            "Door",
            "Hamper",
            "Box",
            "Base Molding",
            "Crown Molding"
        ];

        return parts.Where(p => productPartTypes.Contains(p.PartType));

    }

    private static OrderLoadingSettings GetOrderLoadingSettings(string sourceObj) {
        var sourceObjParts = sourceObj.Split('*');

        if (sourceObjParts.Length != 3) {
            throw new InvalidOperationException("Invalid data source");
        }

        string source = sourceObjParts[0];
        string? customOrderNumber = string.IsNullOrWhiteSpace(sourceObjParts[1]) ? null : sourceObjParts[1];
        string? customWorkingDirectoryRoot = string.IsNullOrWhiteSpace(sourceObjParts[2]) ? null : sourceObjParts[2];
        var settings = new OrderLoadingSettings(source, customOrderNumber, customWorkingDirectoryRoot);
        return settings;
    }

    private async Task<string> GetOrderNumber(string? customOrderNumber, CompanyCustomer customer) {
        string orderNumber;
        if (customOrderNumber is null && string.IsNullOrWhiteSpace(customOrderNumber)) {
            orderNumber = await GetNextOrderNumber(customer.Id);
            var orderNumberPrefix = await _getCustomerOrderPrefixByIdAsync(customer.Id) ?? "";
            orderNumber = $"{orderNumberPrefix}{orderNumber}";
        } else {
            orderNumber = customOrderNumber;
        }

        return orderNumber;
    }

    public static string GetOrderName(string orderName) {
        int start = orderName.IndexOf('-');
        if (start != -1) {
            orderName = orderName[(start + 1)..];
        }
		return orderName;
    }

    private static IProduct CreateProductFromClosetProProduct(IClosetProProduct product, ClosetProSettings settings, ComponentBuilderFactory factory) {

		if (product is CornerShelf cornerShelf) {

			return cornerShelf.ToProduct(settings);

		} else if (product is DrawerBox db) {

			return db.ToProduct(factory, settings);

		} else if (product is FivePieceFront fivePieceFront) {

			return fivePieceFront.ToProduct();

		} else if (product is HutchVerticalPanel hutch) {

			return hutch.ToProduct(settings.VerticalPanelBottomRadius);

		} else if (product is IslandVerticalPanel island) {

			return island.ToProduct();

		} else if (product is MDFFront mdfFront) {

			return mdfFront.ToProduct();

		} else if (product is MelamineSlabFront melaSlab) {

			return melaSlab.ToProduct();

		} else if (product is MiscellaneousClosetPart misc) {

			return misc.ToProduct(settings);

		} else if (product is Shelf shelf) {

			return shelf.ToProduct(settings);

		} else if (product is TransitionVerticalPanel transition) {

			return transition.ToProduct(settings.VerticalPanelBottomRadius);

		} else if (product is VerticalPanel vertical) {

			return vertical.ToProduct(settings.VerticalPanelBottomRadius);

		} else if (product is ZargenDrawerBox zargen) {

			return zargen.ToProduct();

		} else if (product is DividerShelf dividerShelf) {

			return dividerShelf.ToProduct();

		} else if (product is DividerVerticalPanel dividerPanel) {

			return dividerPanel.ToProduct();

		} else {

			throw new InvalidOperationException("Unexpected product");

		}

	}

	private static (DrawerSlide[], Supply[]) GetDrawerSlidesFromProducts(IEnumerable<IProduct> products) {

		var slides = products.OfType<IDrawerSlideContainer>()
								.SelectMany(d => d.GetDrawerSlides())
								.GroupBy(d => (d.Length, d.Style))
								.Select(g => new DrawerSlide(Guid.NewGuid(), g.Sum(g => g.Qty), g.Key.Length, g.Key.Style))
								.ToArray();

		Supply[] screws = [];
		if (slides.Length != 0) {

			int totalSlides = slides.Sum(s => s.Qty);

			screws = [
				Supply.DrawerSlideEuroScrews(slides.Length * 2 * totalSlides),
				Supply.ClosetDrawerClips(slides.Length * totalSlides)
			];

		}

		return (slides, screws);
	}

	private static List<Supply> GetSupplies(IEnumerable<IClosetProProduct> products) {

        List<Supply> supplies = [];

        // TODO: need to check if divider panels need cams

        var shelves = products.OfType<Shelf>().ToArray();
        var corners = products.OfType<CornerShelf>().ToArray();

        // TODO: need to check if the adjustable shelves have pins or not
        int adjPins = shelves.Where(s => s.Type == ShelfType.Adjustable).Sum(s => s.Qty * 4);
        adjPins += shelves.Where(s => s.Type == ShelfType.Shoe).Sum(s => s.Qty * 4);
        adjPins += corners.Where(s => s.Type == CornerShelfType.LAdjustable || s.Type == CornerShelfType.DiagonalAdjustable).Sum(s => s.Qty * 6);
        // Closet spreadsheet adds an additional 4%
        if (adjPins > 0) {
            supplies.Add(Supply.LockingShelfPeg((int)(adjPins * 1.04)));
        }

        int cams = shelves.Where(s => s.Type == ShelfType.Fixed).Sum(s => s.Qty * 4);
        cams += corners.Where(s => s.Type == CornerShelfType.LFixed || s.Type == CornerShelfType.DiagonalFixed).Sum(s => s.Qty * 6);
        // TODO: check that toe kicks are fixed
        cams += products.OfType<MiscellaneousClosetPart>().Where(p => p.Type == MiscellaneousType.ToeKick).Sum(t => t.Qty * 4);
        cams += 8; // The closet spreadsheet add 8 extra cams
        if (cams > 0) {
            supplies.Add(Supply.RafixCam(cams));
        }

        var verticals = products.OfType<VerticalPanel>().ToArray();
        var drilledThrough = verticals.Where(v => v.Drilling == VerticalPanelDrilling.DrilledThrough).Sum(v => v.Qty);
        var finishedSide = verticals.Where(v => v.Drilling != VerticalPanelDrilling.DrilledThrough).Sum(v => v.Qty);
        // Closet spreadsheet adds an additional 5%
        if (finishedSide > 0) {
            supplies.Add(Supply.CamBolt((int)(finishedSide * 6 * 1.05)));
        }
        if (drilledThrough > 0) {
            supplies.Add(Supply.CamBoltDoubleSided((int)(drilledThrough * 6 * 1.05)));
        }

        supplies.AddRange(GetHangingRailSupplies(products));

        return supplies;

    }

    private static IEnumerable<Supply> GetHangingRailSupplies(IEnumerable<IClosetProProduct> products) {

        var verticals = products.OfType<VerticalPanel>().ToArray();
        var wallHung = verticals.Where(v => v.WallHung).ToArray();

		if (wallHung.Length == 0) return [];

		var rooms = products.GroupBy(p => p.Room)
							.Where(g => g.OfType<VerticalPanel>().Any(v => v.WallHung))
							.ToArray();
		double shelfLengths = rooms.Select(r => r.OfType<Shelf>().FirstOrDefault()?.Width ?? Dimension.Zero).Sum(d => d.AsInches());
		double panelThickness = (rooms.Length + 1) * 0.75;
		double totalLength = (shelfLengths + panelThickness) / 12.0;

        var finLeft = wallHung.Where(v => v.Drilling == VerticalPanelDrilling.FinishedLeft).Sum(v => v.Qty);
        var finRight = wallHung.Where(v => v.Drilling == VerticalPanelDrilling.FinishedRight).Sum(v => v.Qty);
        var drilledThrough = wallHung.Where(v => v.Drilling == VerticalPanelDrilling.DrilledThrough).Sum(v => v.Qty);

        return [
            .. Supply.HangingBracketLH(finLeft + (drilledThrough / 2)),
            .. Supply.HangingBracketRH(finRight + (drilledThrough / 2)),
            .. Supply.HangingRail((int) totalLength),
            Supply.LongEuroScrews((finLeft + finRight + drilledThrough) * 2),
        ];

    }

    private async Task<string> CreateWorkingDirectory(string csvData, string company, string orderName, string orderNumber, string? customerWorkingDirectoryRoot) {

		string cpDefaultWorkingDirectory = @"R:\Job Scans\ClosetProSoftware"; // TODO: Get base directory from configuration file
		string workingDirectory = Path.Combine((customerWorkingDirectoryRoot ?? cpDefaultWorkingDirectory), _fileReader.RemoveInvalidPathCharacters($"{orderNumber} - {company} - {orderName}", ' '));

		if (TryToCreateWorkingDirectory(workingDirectory, out string? incomingDir) && incomingDir is not null) {
			string dataFile = _fileReader.GetAvailableFileName(incomingDir, "Incoming", ".csv");
			await File.WriteAllTextAsync(dataFile, csvData);
		}

		return workingDirectory;
	}

    private async Task<string?> GetWorkingDirectoryRoot(string? customWorkingDirectoryRoot, CompanyCustomer customer) {
        string? workingDirectoryRoot;
        if (customWorkingDirectoryRoot is null) {
            workingDirectoryRoot = await _getCustomerWorkingDirectoryRootByIdAsync(customer.Id);
        } else {
            workingDirectoryRoot = customWorkingDirectoryRoot;
        }

        return workingDirectoryRoot;

    }

	private bool TryToCreateWorkingDirectory(string workingDirectory, out string? incomingDirectory) {

		workingDirectory = workingDirectory.Trim();

		try {

			if (Directory.Exists(workingDirectory)) {
				incomingDirectory = CreateSubDirectories(workingDirectory);
				return true;
			} else if (Directory.CreateDirectory(workingDirectory).Exists) {
				incomingDirectory = CreateSubDirectories(workingDirectory);
				return true;
			} else {
				incomingDirectory = null;
				return false;
			}

		} catch (Exception ex) {
			incomingDirectory = null;
			OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Warning, $"Could not create working directory {workingDirectory} - {ex.Message}");
		}

		return false;

	}

	private async Task<string> GetNextOrderNumber(Guid customerId) {

		using var connection = await _dbConnectionFactory.CreateConnection();

		connection.Open();
		var trx = connection.BeginTransaction();

		try {

			var newNumber = connection.QuerySingleOrDefault<int?>("SELECT number FROM order_numbers WHERE customer_id = @CustomerId;", new {
				CustomerId = customerId
			});

			if (newNumber is null) {
				int initialNumber = 1;
				connection.Execute("INSERT INTO order_numbers (customer_id, number) VALUES (@CustomerId, @InitialNumber);", new {
					CustomerId = customerId,
					InitialNumber = initialNumber
				}, trx);
				newNumber = initialNumber;
			}

			connection.Execute("UPDATE order_numbers SET number = @IncrementedValue WHERE customer_id = @CustomerId", new {
				CustomerId = customerId,
				IncrementedValue = newNumber + 1
			});

			trx.Commit();

			return newNumber?.ToString() ?? "0";

		} catch {
			trx.Rollback();
			throw;
		} finally {
			connection.Close();
		}

	}

	private async Task<CompanyCustomer> GetOrCreateCustomer(string designerCompanyName, string designerName) {

		Guid? customerId = await _getCustomerIdByNameAsync(designerCompanyName);

		if (customerId is Guid id) {

			var customer = await _getCustomerByIdAsync(id);
			if (customer is null) {
				throw new InvalidOperationException("Unable to load customer information");
			}
			return customer;

		} else {

			var contact = new Contact() {
				Name = designerName,
				Email = null,
				Phone = null
			};

			var newCustomer = CompanyCustomer.Create(designerCompanyName, "Pick Up", contact, new(), contact, new());

			await _insertCustomerAsync(newCustomer);

			return newCustomer;

		}

	}

	private static string? CreateSubDirectories(string workingDirectory) {
		var cutListDir = Path.Combine(workingDirectory, "CUTLIST");
		_ = Directory.CreateDirectory(cutListDir);

		var ordersDir = Path.Combine(workingDirectory, "orders");
		_ = Directory.CreateDirectory(ordersDir);

		var incomingDir = Path.Combine(workingDirectory, "incoming");
		return Directory.CreateDirectory(incomingDir).Exists ? incomingDir : null;
	}

	public static bool TryParseMoneyString(string text, out decimal value) {
		return decimal.TryParse(text.Replace("$", ""), out value);
	}

	public record FrontHardware(string Name, Dimension Spread);

	private record OrderLoadingSettings(string FilePath, string? CustomOrderNumber, string? CustomWorkingDirectoryRoot);

}
