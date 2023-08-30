using ApplicationCore.Features.Orders.OrderLoading.Dialog;
using ApplicationCore.Features.Orders.OrderLoading.LoadClosetOrderSpreadsheetOrderData.Models;
using ApplicationCore.Features.Orders.OrderLoading.Models;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Doors;
using ApplicationCore.Features.Orders.Shared.Domain.Products.DrawerBoxes;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared;
using ApplicationCore.Shared.Data.Ordering;
using ApplicationCore.Shared.Domain;
using ApplicationCore.Shared.Services;
using Dapper;
using Microsoft.Office.Interop.Excel;
using static ApplicationCore.Features.Companies.Contracts.CompanyDirectory;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadClosetOrderSpreadsheetOrderData;

public class ClosetSpreadsheetOrderProvider : IOrderProvider {

	public IOrderLoadWidgetViewModel? OrderLoadingViewModel { get; set; }

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

	public async Task<OrderData?> LoadOrderData(string data) {

        var parts = data.Split('*');

        if (parts.Length != 3) {
            throw new InvalidOperationException("Invalid data source");
        }

        string source = parts[0];
        string? customOrderNumber = string.IsNullOrWhiteSpace(parts[1]) ? null : parts[1];
        string? customWorkingDirectoryRoot = string.IsNullOrWhiteSpace(parts[2]) ? null : parts[2];

        if (!_fileReader.DoesFileExist(source)) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "Could not access given filepath");
            return null;
        }

        var extension = Path.GetExtension(source);
        if (extension is null || extension != ".xlsx" && extension != ".xlsm") {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "Given filepath is not an excel document");
            return null;
        }

        Microsoft.Office.Interop.Excel.Application? app = null;
        Workbook? workbook = null;

        try {

            app = new() {
                DisplayAlerts = false,
                Visible = false
            };

            workbook = app.Workbooks.Open(source, ReadOnly: true);
			if (!TryGetWorksheet(workbook, "Cover", out Worksheet? coverSheet) || coverSheet is null) return null;
			if (!TryGetWorksheet(workbook, "Closet Parts", out Worksheet? closetPartSheet) || closetPartSheet is null) return null;
			if (!TryGetWorksheet(workbook, "Corner Shelves", out Worksheet? cornerShelfSheet) || cornerShelfSheet is null) return null;
			if (!TryGetWorksheet(workbook, "Zargen", out Worksheet? zargenSheet) || zargenSheet is null) return null;
			if (!TryGetWorksheet(workbook, "Dovetail", out Worksheet? dovetailSheet) || dovetailSheet is null) return null;
			if (!TryGetWorksheet(workbook, "Melamine DB", out Worksheet? melaDbSheet) || melaDbSheet is null) return null;
			if (!TryGetWorksheet(workbook, "MDF Fronts", out Worksheet? mdfFrontSheet) || mdfFrontSheet is null) return null;

            var cover = Cover.ReadFromWorksheet(coverSheet);

            Guid? customerId = await _getCustomerIdByNameAsync(cover.CustomerName);

            if (customerId is null) {
                OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"Could not find customer");
                return null;
            }

            string orderNumber;
            if (customOrderNumber is null && string.IsNullOrWhiteSpace(customOrderNumber)) {
                orderNumber = await GetNextOrderNumber((Guid) customerId);
                var orderNumberPrefix = await _getCustomerOrderPrefixByIdAsync((Guid) customerId) ?? throw new InvalidOperationException("Could not get customer data");
                orderNumber = $"{orderNumberPrefix}{orderNumber}";
            } else {
                orderNumber = customOrderNumber;
            }

            string? workingDirectoryRoot = customWorkingDirectoryRoot;
            if (workingDirectoryRoot is null) {
                workingDirectoryRoot = await _getCustomerWorkingDirectoryRootByIdAsync((Guid) customerId);
            }
            if (workingDirectoryRoot is null) {
                workingDirectoryRoot = @"R\Job Scans\Closets";
                return null;
            }
            string workingDirectory = Path.Combine(workingDirectoryRoot, _fileReader.RemoveInvalidPathCharacters($"{orderNumber} {cover.JobName}", ' '));
            if (!TryToCreateWorkingDirectory(workingDirectory, out string? incomingDirectory)) {
                OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"Could not create working directory");
                return null;
            }
            if (incomingDirectory is not null) {
                string fileName = Path.GetFileName(source);
                File.Copy(source, Path.Combine(incomingDirectory, fileName));
            }

            var closetParts = LoadItemsFromWorksheet<ClosetPart>(closetPartSheet);
            var cornerShelves = LoadItemsFromWorksheet<CornerShelf>(cornerShelfSheet);
            var zargens = LoadItemsFromWorksheet<Zargen>(zargenSheet);

            var dovetailDBHeader = DovetailDBHeader.ReadFromWorksheet(dovetailSheet);
			var dovetailDBs = LoadItemsFromWorksheet<DovetailDB>(dovetailSheet);

            var melamineDBHeader = MelamineDBHeader.ReadFromWorksheet(melaDbSheet);
			var melamineDBs = LoadItemsFromWorksheet<MelamineDB>(melaDbSheet);

            var mdfFrontHeader = MDFFrontHeader.ReadFromWorksheet(mdfFrontSheet);
            var mdfFronts = LoadItemsFromWorksheet<MDFFront>(mdfFrontSheet);

            List<IProduct> products = new();
            products.AddRange(MapClosetPartToProduct(cover, closetParts));
            products.AddRange(MapCornerShelfToProduct(cover, cornerShelves));
            products.AddRange(MapZargenToProduct(cover, zargens));
            products.AddRange(MapDovetailDBToProduct(dovetailDBHeader, dovetailDBs));
            products.AddRange(MapMelamineDBToProduct(melamineDBHeader, melamineDBs));
            products.AddRange(MapMDFFrontToProduct(mdfFrontHeader, mdfFronts));

            List<AdditionalItem> additionalItems = new();
            if (cover.InstallCamsCharge > 0) {
                additionalItems.Add(AdditionalItem.Create("Install Cams", cover.InstallCamsCharge, true));
            }
            if (cover.RushCharge > 0) {
                additionalItems.Add(AdditionalItem.Create("Rush", cover.RushCharge, true));
            }
            cover.Moldings
                .Where(m => m.LinearFt > 0)
                .Select(m => AdditionalItem.Create($"{m.Name} - {m.Color} - {m.LinearFt}ft", m.Price))
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
                CustomerId = (Guid) customerId,
                VendorId = vendorId,
                AdditionalItems = additionalItems,
                Products = products,
                Rush = cover.RushCharge > 0,
                Info = info
            };

        } catch (Exception ex) {

            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"Error occurred while reading order from workbook {ex}");

        } finally {

            workbook?.Close(SaveChanges: false);
            app?.Quit();

            // Clean up COM objects, calling these twice ensures it is fully cleaned up.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();

        }

        return null;

	}
    
    private IEnumerable<IProduct> MapClosetPartToProduct(Cover cover, IEnumerable<ClosetPart> closetParts) {

        foreach (var closetPart in closetParts) {

            var parameters = new Dictionary<string, string>() {
                { "FinLeft", closetPart.Finished == "Left" ? "1" : "0" },
                { "FinRight", closetPart.Finished == "Right" ? "1" : "0" },
                { "WallMount", closetPart.WallMount == "Yes" ? "1" : "0" }
            };

            yield return new Shared.Domain.Products.Closets.ClosetPart(Guid.NewGuid(), closetPart.Qty, closetPart.UnitPrice, closetPart.Number, closetPart.RoomName, closetPart.Item, Dimension.FromMillimeters(closetPart.Width), Dimension.FromMillimeters(closetPart.Length), new(cover.MaterialColor, Shared.Domain.Enums.ClosetMaterialCore.ParticleBoard), null, cover.MaterialColor, closetPart.Comment, parameters);

        }

    }
 
    private IEnumerable<IProduct> MapCornerShelfToProduct(Cover cover, IEnumerable<CornerShelf> cornerShelves) {

        foreach (var cornerShelf in cornerShelves) {

            var parameters = new Dictionary<string, string>() {
                { "RightWidth", cornerShelf.RightWidth.ToString() },
                { "NotchSideLength", cornerShelf.NotchSideLength.ToString() },
                { "NotchLeft", cornerShelf.NotchSide == "L" ? "1" : "0" },
                { "ShelfRadius", cornerShelf.ShelfRadius.ToString() }
            };

            yield return new Shared.Domain.Products.Closets.ClosetPart(Guid.NewGuid(), cornerShelf.Qty, cornerShelf.UnitPrice, cornerShelf.Number, cornerShelf.RoomName, cornerShelf.Item, Dimension.FromMillimeters(cornerShelf.ProductWidth), Dimension.FromMillimeters(cornerShelf.ProductLength), new(cover.MaterialColor, Shared.Domain.Enums.ClosetMaterialCore.ParticleBoard), null, cover.MaterialColor, cornerShelf.Comment, parameters);

        }

    }

    private IEnumerable<IProduct> MapZargenToProduct(Cover cover, IEnumerable<Zargen> zargens) {

        int line = 1;
        
        foreach (var zargen in zargens) {

            if (!zargen.Item.StartsWith('D') || !double.TryParse(zargen.Item[1..], out double height)) {
                OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"Unexpected zargen item name {zargen.Item} on line {line}");
                continue;
            }

            var parameters = new Dictionary<string, string>() {
                { "OpeningWidth", zargen.HoleSize.ToString() },
                { "PullCenters", zargen.PullCtrDim.ToString() },
            };

            yield return new Shared.Domain.Products.Closets.ZargenDrawer(Guid.NewGuid(), zargen.Qty, zargen.ExtPrice / zargen.Qty, line++, string.Empty, zargen.Item, Dimension.FromMillimeters(zargen.HoleSize), Dimension.FromMillimeters(height), Dimension.FromMillimeters(zargen.SlideDepth), new(cover.MaterialColor, Shared.Domain.Enums.ClosetMaterialCore.ParticleBoard), null, cover.MaterialColor, string.Empty, parameters);

        }

    }

    private IEnumerable<IProduct> MapMelamineDBToProduct(MelamineDBHeader header, IEnumerable<MelamineDB> melamineDBs) {

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
													 Dimension.Zero);

        }

    }

    private IEnumerable<IProduct> MapDovetailDBToProduct(DovetailDBHeader header, IEnumerable<DovetailDB> dovetailDBs) {

        string accessory = header.IncludeHettichSlides ? "Hettich Slides" : "";

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
															 header.Notch,
															 accessory,
															 Shared.Domain.Enums.LogoPosition.None,
															 header.PostFinish));

        }

    }

    private IEnumerable<IProduct> MapMDFFrontToProduct(MDFFrontHeader header, IEnumerable<MDFFront> mdfFronts) {

        foreach (var front in mdfFronts) {

            yield return MDFDoorProduct.Create(front.UnitPrice,
											   "",
											   front.Qty,
											   front.LineNumber,
											   Shared.Domain.Enums.DoorType.Door,
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
											   string.IsNullOrWhiteSpace(header.PaintColor) ? null : "");

        }

    }

	private IEnumerable<T> LoadItemsFromWorksheet<T>(Worksheet worksheet) where T : IWorksheetReadable<T>, new() {

		List<T> items = new();

		int row = T.FirstRow;
		while (true) {

			var lineVal = worksheet.Range[$"B{row}"].Value2;
			if (lineVal is null || lineVal.ToString() == "") {
				break;
			}

			try {
				items.Add(T.ReadFromWorksheet(worksheet, row));
			} catch (Exception ex) {
				OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"Error reading item at row {row}");
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

	private bool TryGetWorksheet(Workbook workbook, string name,  out Worksheet? worksheet) {
        worksheet = (Worksheet?)workbook.Sheets[name];
        if (worksheet is null) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"Could not find sheet '{name}' in workbook");
            return false;
        }
        return true;
    }

    private bool TryToCreateWorkingDirectory(string workingDirectory, out string? incomingDirectory) {

        workingDirectory = workingDirectory.Trim();

        try {

            if (Directory.Exists(workingDirectory)) {
                incomingDirectory = CreateSubDirectories(workingDirectory);
                return true;
            }else if (Directory.CreateDirectory(workingDirectory).Exists) {
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

            var newNumber = await connection.QuerySingleOrDefaultAsync<int?>("SELECT number FROM order_numbers WHERE customer_id = @CustomerId;", new {
                CustomerId = customerId
            });

            if (newNumber is null) {
                int initialNumber = 1;
                await connection.ExecuteAsync("INSERT INTO order_numbers (customer_id, number) VALUES (@CustomerId, @InitialNumber);", new {
                    CustomerId = customerId,
                    InitialNumber = initialNumber
                }, trx);
                newNumber = initialNumber;
            }

            await connection.ExecuteAsync("UPDATE order_numbers SET number = @IncrementedValue WHERE customer_id = @CustomerId", new {
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

}
