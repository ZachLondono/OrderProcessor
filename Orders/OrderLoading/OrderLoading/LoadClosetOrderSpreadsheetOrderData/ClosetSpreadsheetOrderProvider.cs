using OrderLoading.LoadClosetOrderSpreadsheetOrderData.Models;
using Domain.Orders.Entities;
using Domain.Orders.ValueObjects;
using Domain.Extensions;
using Domain.ValueObjects;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using static Domain.Companies.CompanyDirectory;
using Domain.Orders.Entities.Products;
using Domain.Orders.Entities.Products.Doors;
using Domain.Orders.Entities.Products.DrawerBoxes;
using Domain.Orders.Persistance;
using Domain.Services;
using static OrderLoading.IOrderProvider;
using OneOf.Types;

namespace OrderLoading.LoadClosetOrderSpreadsheetOrderData;

public class ClosetSpreadsheetOrderProvider : IOrderProvider {

	public SourceData? Source { get; set; } = null;

	private readonly IFileReader _fileReader;
	private readonly GetCustomerOrderPrefixByIdAsync _getCustomerOrderPrefixByIdAsync;
	private readonly GetCustomerWorkingDirectoryRootByIdAsync _getCustomerWorkingDirectoryRootByIdAsync;
	private readonly GetCustomerIdByNameAsync _getCustomerIdByNameAsync;
	private readonly IOrderingDbConnectionFactory _dbConnectionFactory; // TODO: don't reference this directly, create a delegate like the ones above

	public ClosetSpreadsheetOrderProvider(IFileReader fileReader, GetCustomerOrderPrefixByIdAsync getCustomerOrderPrefixByIdAsync, GetCustomerWorkingDirectoryRootByIdAsync getCustomerWorkingDirectoryRootByIdAsync, IOrderingDbConnectionFactory dbConnectionFactory, GetCustomerIdByNameAsync getCustomerIdByNameAsync) {
		_fileReader = fileReader;
		_getCustomerOrderPrefixByIdAsync = getCustomerOrderPrefixByIdAsync;
		_getCustomerWorkingDirectoryRootByIdAsync = getCustomerWorkingDirectoryRootByIdAsync;
		_dbConnectionFactory = dbConnectionFactory;
		_getCustomerIdByNameAsync = getCustomerIdByNameAsync;
	}

	public async Task<OrderData?> LoadOrderData(LogProgress logProgress) {

		if (Source is null) {
			throw new InvalidOperationException("Invalid data source");
		}

		string? customOrderNumber = string.IsNullOrWhiteSpace(Source.OrderNumber) ? null : Source.OrderNumber;
		string? customWorkingDirectoryRoot = string.IsNullOrWhiteSpace(Source.WorkingDirectoryRoot) ? null : Source.WorkingDirectoryRoot;

		if (!_fileReader.DoesFileExist(Source.FilePath)) {
			logProgress(MessageSeverity.Error, "Could not access given file path");
			return null;
		}

		var extension = Path.GetExtension(Source.FilePath);
		if (extension is null || extension != ".xlsx" && extension != ".xlsm") {
			logProgress(MessageSeverity.Error, "Given file path is not an excel document");
			return null;
		}

		Application? app = null;
		Workbooks? workbooks = null;
		Workbook? workbook = null;

		try {

			app = new() {
				DisplayAlerts = false,
				Visible = false
			};

			workbooks = app.Workbooks;
			workbook = workbooks.Open(Source.FilePath, ReadOnly: true);
			if (!TryGetWorksheet(workbook, "Cover", out Worksheet? coverSheet, logProgress) || coverSheet is null) return null;
			if (!TryGetWorksheet(workbook, "Closet Parts", out Worksheet? closetPartSheet, logProgress) || closetPartSheet is null) return null;
			if (!TryGetWorksheet(workbook, "Corner Shelves", out Worksheet? cornerShelfSheet, logProgress) || cornerShelfSheet is null) return null;
			if (!TryGetWorksheet(workbook, "Zargen", out Worksheet? zargenSheet, logProgress) || zargenSheet is null) return null;
			if (!TryGetWorksheet(workbook, "Dovetail", out Worksheet? dovetailSheet, logProgress) || dovetailSheet is null) return null;
			if (!TryGetWorksheet(workbook, "Melamine DB", out Worksheet? melaDbSheet, logProgress) || melaDbSheet is null) return null;
			if (!TryGetWorksheet(workbook, "MDF Fronts", out Worksheet? mdfFrontSheet, logProgress) || mdfFrontSheet is null) return null;

			var cover = Cover.ReadFromWorksheet(coverSheet);

			Guid? customerId = await _getCustomerIdByNameAsync(cover.CustomerName);

			if (customerId is null) {
				logProgress(MessageSeverity.Error, $"Could not find customer");
				return null;
			}

			string orderNumber;
			if (customOrderNumber is null && string.IsNullOrWhiteSpace(customOrderNumber)) {
				orderNumber = await GetNextOrderNumber((Guid)customerId);
				var orderNumberPrefix = await _getCustomerOrderPrefixByIdAsync((Guid)customerId) ?? "";
				orderNumber = $"{orderNumberPrefix}{orderNumber}";
			} else {
				orderNumber = customOrderNumber;
			}

			string? workingDirectoryRoot = customWorkingDirectoryRoot;
			if (workingDirectoryRoot is null) {
				workingDirectoryRoot = await _getCustomerWorkingDirectoryRootByIdAsync((Guid)customerId);
			}
			if (workingDirectoryRoot is null) {
				workingDirectoryRoot = @"R\Job Scans\Closets";
				return null;
			}
			string workingDirectory = Path.Combine(workingDirectoryRoot, _fileReader.RemoveInvalidPathCharacters($"{orderNumber} {cover.JobName}", ' '));
			if (!TryToCreateWorkingDirectory(workingDirectory, out string? incomingDirectory, logProgress)) {
				logProgress(MessageSeverity.Error, $"Could not create working directory");
				return null;
			}
			if (incomingDirectory is not null) {
				string fileName = Path.GetFileName(Source.FilePath);
				File.Copy(Source.FilePath, Path.Combine(incomingDirectory, fileName));
			}

			var closetParts = LoadItemsFromWorksheet<ClosetPart>(closetPartSheet, logProgress);
			var cornerShelves = LoadItemsFromWorksheet<CornerShelf>(cornerShelfSheet, logProgress);
			var zargens = LoadItemsFromWorksheet<Zargen>(zargenSheet, logProgress);

			var dovetailDBHeader = DovetailDBHeader.ReadFromWorksheet(dovetailSheet);
			var dovetailDBs = LoadItemsFromWorksheet<DovetailDB>(dovetailSheet, logProgress);

			var melamineDBHeader = MelamineDBHeader.ReadFromWorksheet(melaDbSheet);
			var melamineDBs = LoadItemsFromWorksheet<MelamineDB>(melaDbSheet, logProgress);

			var mdfFrontHeader = MDFFrontHeader.ReadFromWorksheet(mdfFrontSheet);
			var mdfFronts = LoadItemsFromWorksheet<MDFFront>(mdfFrontSheet, logProgress);

			bool installCams = cover.InstallCamsCharge > 0;

			List<IProduct> products = [
				.. MapClosetPartToProduct(cover, closetParts, installCams),
				.. MapCornerShelfToProduct(cover, cornerShelves, installCams),
				.. MapZargenToProduct(cover, zargens, logProgress),
				.. MapDovetailDBToProduct(dovetailDBHeader, dovetailDBs),
				.. MapMelamineDBToProduct(melamineDBHeader, melamineDBs),
				.. MapMDFFrontToProduct(mdfFrontHeader, mdfFronts),
			];

			List<AdditionalItem> additionalItems = [];

			if (cover.InstallCamsCharge > 0) {
				additionalItems.Add(AdditionalItem.Create(1, "Install Cams", cover.InstallCamsCharge));
			}
			if (cover.RushCharge > 0) {
				additionalItems.Add(AdditionalItem.Create(1, "Rush", cover.RushCharge));
			}

			cover.Moldings
				.Where(m => m.LinearFt > 0)
				.Select(m => AdditionalItem.Create(1, $"{m.Name} - {m.Color} - {m.LinearFt}ft", m.Price))
				.ForEach(additionalItems.Add);

			var address = ParseCustomerAddress(cover.AddressLine1, cover.AddressLine2);

			var billing = new BillingInfo() {
				InvoiceEmail = cover.CustomerEmail,
				PhoneNumber = cover.CustomerPhone,
				Address = address
			};

			ShippingInfo shipping = new() {
				Contact = "",
				Method = cover.ShippingInformation,
				PhoneNumber = cover.CustomerPhone,
				Price = cover.DeliveryCharge,
				Address = address
			};

			var info = new Dictionary<string, string>();
			var vendorId = Guid.Parse("579badff-4579-481d-98cf-0012eb2cc75e");

			return new() {
				Number = orderNumber,
				Name = cover.JobName,
				WorkingDirectory = workingDirectory,                                         // TODO: Get default working directory from configuration file
				Comment = cover.SpecialRequirements,
				Shipping = shipping,
				Billing = billing,
				Tax = cover.Tax,
				PriceAdjustment = cover.ManualCharge,
				OrderDate = cover.OrderDate,
				DueDate = cover.DueDate,
				CustomerId = (Guid)customerId,
				VendorId = vendorId,
				AdditionalItems = additionalItems,
				Products = products,
				Rush = cover.RushCharge > 0,
				Info = info,
                Hardware = Hardware.None()
            };

		} catch (Exception ex) {

			logProgress(MessageSeverity.Error, $"Error occurred while reading order from workbook {ex}");

		} finally {

			workbook?.Close(SaveChanges: false);
			workbooks?.Close();
			app?.Quit();

			if (workbook is not null) _ = Marshal.ReleaseComObject(workbook);
			if (workbooks is not null) _ = Marshal.ReleaseComObject(workbooks);
			if (app is not null) _ = Marshal.ReleaseComObject(app);

			// Clean up COM objects, calling these twice ensures it is fully cleaned up.
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			GC.WaitForPendingFinalizers();

		}

		return null;

	}

	private static IEnumerable<IProduct> MapClosetPartToProduct(Cover cover, IEnumerable<ClosetPart> closetParts, bool installCams) {

		foreach (var closetPart in closetParts) {

			var parameters = new Dictionary<string, string>() {
				{ "FinLeft", closetPart.Finished == "Left" ? "1" : "0" },
				{ "FinRight", closetPart.Finished == "Right" ? "1" : "0" },
				{ "WallMount", closetPart.WallMount == "Yes" ? "1" : "0" }
			};

			yield return new Domain.Orders.Entities.Products.Closets.ClosetPart(Guid.NewGuid(),
                                                                       closetPart.Qty,
                                                                       closetPart.UnitPrice,
                                                                       closetPart.Number,
                                                                       closetPart.RoomName,
                                                                       closetPart.Item,
                                                                       Dimension.FromMillimeters(closetPart.Width),
                                                                       Dimension.FromMillimeters(closetPart.Length),
                                                                       new(cover.MaterialColor, Domain.Orders.Enums.ClosetMaterialCore.ParticleBoard),
                                                                       null,
                                                                       cover.MaterialColor,
                                                                       closetPart.Comment,
																	   installCams,
                                                                       parameters);

		}

	}

	private static IEnumerable<IProduct> MapCornerShelfToProduct(Cover cover, IEnumerable<CornerShelf> cornerShelves, bool installCams) {

		foreach (var cornerShelf in cornerShelves) {

			var parameters = new Dictionary<string, string>() {
				{ "RightWidth", cornerShelf.RightWidth.ToString() },
				{ "NotchSideLength", cornerShelf.NotchSideLength.ToString() },
				{ "NotchLeft", cornerShelf.NotchSide == "L" ? "1" : "0" },
				{ "ShelfRadius", cornerShelf.ShelfRadius.ToString() }
			};

			yield return new Domain.Orders.Entities.Products.Closets.ClosetPart(Guid.NewGuid(),
                                                                       cornerShelf.Qty,
                                                                       cornerShelf.UnitPrice,
                                                                       cornerShelf.Number,
                                                                       cornerShelf.RoomName,
                                                                       cornerShelf.Item,
                                                                       Dimension.FromMillimeters(cornerShelf.ProductWidth),
                                                                       Dimension.FromMillimeters(cornerShelf.ProductLength),
                                                                       new(cover.MaterialColor, Domain.Orders.Enums.ClosetMaterialCore.ParticleBoard),
                                                                       null,
                                                                       cover.MaterialColor,
                                                                       cornerShelf.Comment,
																	   installCams,
                                                                       parameters);

		}

	}

	private static IEnumerable<IProduct> MapZargenToProduct(Cover cover, IEnumerable<Zargen> zargens, LogProgress logProgress) {

		int line = 1;

		foreach (var zargen in zargens) {

			if (!zargen.Item.StartsWith('D') || !double.TryParse(zargen.Item[1..], out double height)) {
				logProgress(MessageSeverity.Error, $"Unexpected zargen item name {zargen.Item} on line {line}");
				continue;
			}

			var parameters = new Dictionary<string, string>() {
				{ "OpeningWidth", zargen.HoleSize.ToString() },
				{ "PullCenters", zargen.PullCtrDim.ToString() },
			};

			yield return new Domain.Orders.Entities.Products.Closets.ZargenDrawer(Guid.NewGuid(), zargen.Qty, zargen.ExtPrice / zargen.Qty, line++, string.Empty, zargen.Item, Dimension.FromMillimeters(zargen.HoleSize), Dimension.FromMillimeters(height), Dimension.FromMillimeters(zargen.SlideDepth), new(cover.MaterialColor, Domain.Orders.Enums.ClosetMaterialCore.ParticleBoard), null, cover.MaterialColor, string.Empty, parameters);

		}

	}

	private static IEnumerable<IProduct> MapMelamineDBToProduct(MelamineDBHeader header, IEnumerable<MelamineDB> melamineDBs) {

		foreach (var melamine in melamineDBs) {

			yield return new DoweledDrawerBoxProduct(Guid.NewGuid(),
													 melamine.UnitPrice,
													 melamine.Qty,
													 "",
													 melamine.LineNumber,
													 Dimension.FromInches(melamine.Height),
													 Dimension.FromInches(melamine.Width),
													 Dimension.FromInches(melamine.Depth),
													 new(header.BoxMaterial, Dimension.FromInches(0.75), true),
													 new(header.BoxMaterial, Dimension.FromInches(0.75), true),
													 new(header.BoxMaterial, Dimension.FromInches(0.75), true),
													 new(header.BottomMaterial, Dimension.FromInches(0.75), true),
													 false,
													 Dimension.Zero,
													 DoweledDrawerBoxConfig.NO_NOTCH);

		}

	}

	private static IEnumerable<IProduct> MapDovetailDBToProduct(DovetailDBHeader header, IEnumerable<DovetailDB> dovetailDBs) {

		string slides = header.IncludeHettichSlides ? "Hettich Slides" : "";

		foreach (var dovetail in dovetailDBs) {

			yield return DovetailDrawerBoxProduct.Create(dovetail.UnitPrice,
														 dovetail.Qty,
														 "",
														 dovetail.LineNumber,
														 Dimension.FromInches(dovetail.Height),
														 Dimension.FromInches(dovetail.Width),
														 Dimension.FromInches(dovetail.Depth),
														 dovetail.Note,
														 new Dictionary<string, string>(),
														 new DovetailDrawerBoxConfig(
															 header.BoxMaterial,
															 header.BoxMaterial,
															 header.BoxMaterial,
															 header.BottomMaterial,
															 header.Clips,
															 slides,
															 header.Notch,
															 string.Empty,
															 Domain.Orders.Enums.LogoPosition.None,
															 header.PostFinish));

		}

	}

	private static IEnumerable<IProduct> MapMDFFrontToProduct(MDFFrontHeader header, IEnumerable<MDFFront> mdfFronts) {

		foreach (var front in mdfFronts) {

			yield return MDFDoorProduct.Create(front.UnitPrice,
											   "",
											   front.Qty,
											   front.LineNumber,
											   Domain.Orders.Enums.DoorType.Door,
											   Dimension.FromInches(front.Height),
											   Dimension.FromInches(front.Width),
											   front.Note,
											   new(Dimension.FromInches(header.FrameSize)),
											   "MDF-3/4",
											   Dimension.FromInches(0.75),
											   header.Style,
											   "Square",
											   "Flat",
											   Dimension.Zero,
											   DoorOrientation.Vertical,
											   Array.Empty<AdditionalOpening>(),
											   string.IsNullOrWhiteSpace(header.PaintColor) ? new None() : new Paint(header.PaintColor),
											   new SolidPanel());

		}

	}

	private static IEnumerable<T> LoadItemsFromWorksheet<T>(Worksheet worksheet, LogProgress logProgress) where T : IWorksheetReadable<T>, new() {

		List<T> items = [];

		int row = T.FirstRow;
		while (true) {

			var lineVal = worksheet.Range[$"B{row}"].Value2;
			if (lineVal is null || lineVal.ToString() == "") {
				break;
			}

			try {
				items.Add(T.ReadFromWorksheet(worksheet, row));
			} catch {
				logProgress(MessageSeverity.Error, $"Error reading item at row {row}");
				// TODO: log exception
			}

			row += T.RowStep;

		}

		return items;

	}

	private static Address ParseCustomerAddress(string line1, string line2) {

		var parts = line2.Split(' ').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
		string city = "", state = "", zip = "";
		if (parts.Length == 3) {
			city = parts[0].TrimEnd(',');
			state = parts[1];
			zip = parts[2];
		}

		return new() {
			Line1 = line1,
			Line2 = "",
			Line3 = "",
			City = city,
			State = state,
			Zip = zip,
			Country = "USA"
		};
	}

	private static bool TryGetWorksheet(Workbook workbook, string name, out Worksheet? worksheet, LogProgress logProgress) {
		worksheet = (Worksheet?)workbook.Sheets[name];
		if (worksheet is null) {
			logProgress(MessageSeverity.Error, $"Could not find sheet '{name}' in workbook");
			return false;
		}
		return true;
	}

	private static bool TryToCreateWorkingDirectory(string workingDirectory, out string? incomingDirectory, LogProgress logProgress) {

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
			logProgress(MessageSeverity.Warning, $"Could not create working directory {workingDirectory} - {ex.Message}");
		}

		return false;

	}

	private static string? CreateSubDirectories(string workingDirectory) {
		var cutListDir = Path.Combine(workingDirectory, "CUTLIST");
		_ = Directory.CreateDirectory(cutListDir);

		var ordersDir = Path.Combine(workingDirectory, "orders");
		_ = Directory.CreateDirectory(ordersDir);

		var incomingDir = Path.Combine(workingDirectory, "incoming");
		return Directory.CreateDirectory(incomingDir).Exists ? incomingDir : null;
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

	public record SourceData(string FilePath, string OrderNumber, string WorkingDirectoryRoot);

}
