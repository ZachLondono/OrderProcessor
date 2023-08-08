using ApplicationCore.Features.Companies.Contracts;
using ApplicationCore.Features.Companies.Contracts.Entities;
using ApplicationCore.Features.Companies.Contracts.ValueObjects;
using ApplicationCore.Features.Orders.OrderLoading.Dialog;
using ApplicationCore.Features.Orders.OrderLoading.LoadDoweledDBSpreadsheetOrderData.Models;
using ApplicationCore.Features.Orders.OrderLoading.Models;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.Products.DrawerBoxes;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;
using ApplicationCore.Shared.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Office.Interop.Excel;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadDoweledDBSpreadsheetOrderData;

internal class DoweledDBSpreadsheetOrderProvider : IOrderProvider {

    private readonly IFileReader _fileReader;
    private readonly ILogger<DoweledDBSpreadsheetOrderProvider> _logger;
    private readonly DoweledDBOrderProviderOptions _options;
    private readonly CompanyDirectory.InsertCustomerAsync _insertCustomerAsync;
    private readonly CompanyDirectory.GetCustomerIdByNameAsync _getCustomerByNamAsync;
    private readonly CompanyDirectory.GetCustomerWorkingDirectoryRootByIdAsync _getCustomerWorkingDirectoryRootByIdAsync;

    public IOrderLoadWidgetViewModel? OrderLoadingViewModel { get; set; }

    public DoweledDBSpreadsheetOrderProvider(IFileReader fileReader, ILogger<DoweledDBSpreadsheetOrderProvider> logger, IOptions<DoweledDBOrderProviderOptions> options, CompanyDirectory.InsertCustomerAsync insertCustomerAsync, CompanyDirectory.GetCustomerIdByNameAsync getCustomerByNamAsync, CompanyDirectory.GetCustomerWorkingDirectoryRootByIdAsync getCustomerWorkingDirectoryRootByIdAsync) {
        _fileReader = fileReader;
        _logger = logger;
        _options = options.Value;
        _insertCustomerAsync = insertCustomerAsync;
        _getCustomerByNamAsync = getCustomerByNamAsync;
        _getCustomerWorkingDirectoryRootByIdAsync = getCustomerWorkingDirectoryRootByIdAsync;
    }

    public async Task<OrderData?> LoadOrderData(string source) {

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

            Worksheet? orderSheet = (Worksheet?)workbook.Worksheets["Order"];
            Worksheet? specSheet = (Worksheet?)workbook.Worksheets["Specs"];

            if (orderSheet is null) {
                OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "Could not find Order sheet in workbook");
                return null;
            }

            if (specSheet is null) {
                OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, "Could not find Spec sheet in workbook");
                return null;
            }

            var header = Header.ReadFromSheet(orderSheet);
            var customerInfo = CustomerInfo.ReadFromSheet(orderSheet);

            var slideSpecs = UMSlideSpecs.ReadFromSheet(specSheet);
            var bottomSpecs = BottomSpecs.ReadFromSheet(specSheet);
            var frontDrillingSpecs = FrontDrillingSpecs.ReadFromSheet(specSheet);
            var constructionSpecs = ConstructionSpecs.ReadFromSheet(specSheet);

            bool useInches = header.Units == "English (in)";
            bool machineThicknessForUMSlides = slideSpecs.UMSlideMachining;
            var frontBackHeightAdjustment = Dimension.FromMillimeters(constructionSpecs.FrontBackDrop);
            var boxes = LoadAllLineItems(orderSheet)
                                            .Select(i => i.CreateDoweledDrawerBoxProduct(useInches, machineThicknessForUMSlides, frontBackHeightAdjustment))
                                            .Cast<IProduct>()
                                            .ToList();

            var vendorId = Guid.Parse(_options.VendorIds[header.VendorName]);
            var customerId = await GetCustomerId(header.CustomerName, customerInfo);

            var dirRoot = await _getCustomerWorkingDirectoryRootByIdAsync(customerId);
            var workingDirectory = CreateWorkingDirectory(source, header.OrderNumber, header.OrderName, header.CustomerName, dirRoot);

            return new() {
                VendorId = vendorId,
                CustomerId = customerId,
                Comment = header.SpecialInstructions,
                OrderDate = header.OrderDate,
                Rush = false,
                Name = header.OrderName,
                Number = header.OrderNumber,
                PriceAdjustment = 0,
                Tax = 0,
                WorkingDirectory = workingDirectory,
                AdditionalItems = new(),
                Info = new(),
                Billing = new() {
                    PhoneNumber = "",
                    InvoiceEmail = "",
                    Address = new()
                },
                Shipping = new() {
                    Contact = "",
                    Method = "",
                    PhoneNumber = "",
                    Price = 0,
                    Address = new()
                },
                Products = boxes 
            };

        } catch (Exception ex) {

            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"Error occurred while reading order from workbook {ex}");
            _logger.LogError(ex, "Exception thrown while loading order from workbook");

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

    private IEnumerable<LineItem> LoadAllLineItems(Worksheet worksheet) {

        List<LineItem> lineItems = new();

        int row = 0;
        while (true) {

            if (!LineItem.DoesRowContainItem(worksheet, row)) {
                break;
            }

            try {

                var line = LineItem.ReadFromSheet(worksheet, row);
    
                lineItems.Add(line);

            } catch (Exception ex) {

                OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"Error reading line item at line {row}");
                _logger.LogError(ex, "Exception thrown while reading line item from workbook");

            }

            row++;

        }

        return lineItems;

    }

    private async Task<Guid> GetCustomerId(string customerName, CustomerInfo customerInfo) {

        Guid? result = await _getCustomerByNamAsync(customerName);

        if (result is not null) {
            return (Guid)result;
        }

        var address = new Companies.Contracts.ValueObjects.Address() {
            Line1 = customerInfo.Line1,
            Line2 = customerInfo.Line2,
            Line3 = customerInfo.Line3,
            City = customerInfo.City,
            State = customerInfo.State,
            Zip = customerInfo.Zip,
            Country = "USA"
        };

        var billingContact = new Contact() {
            Name = customerInfo.Contact,
            Phone = "",
            Email = customerInfo.Email
        };

        var shippingContact = new Contact() {
            Name = customerInfo.Contact,
            Phone = "",
            Email = customerInfo.Email
        };

        var customer = Customer.Create(customerName, string.Empty, shippingContact, address, billingContact, address);
        await _insertCustomerAsync(customer);

        return customer.Id;

    }

    private string CreateWorkingDirectory(string source, string orderNumber, string orderName, string customerName, string? customerWorkingDirectory) {
        string workingDirectory = Path.Combine((customerWorkingDirectory ?? _options.DefaultWorkingDirectory), _fileReader.RemoveInvalidPathCharacters($"{orderNumber} - {customerName} - {orderName}", ' '));
        bool workingDirExists = TryToCreateWorkingDirectory(workingDirectory);
        if (workingDirExists) {
            string dataFile = _fileReader.GetAvailableFileName(workingDirectory, "Incoming", ".csv");
            File.Copy(source, dataFile);
        }

        return workingDirectory;
    }

    private bool TryToCreateWorkingDirectory(string workingDirectory) {

        workingDirectory = workingDirectory.Trim();

        if (Directory.Exists(workingDirectory)) {
            return true;
        }

        try {
            var dirInfo = Directory.CreateDirectory(workingDirectory);
            return dirInfo.Exists;
        } catch (Exception ex) {
            OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Warning, $"Could not create working directory {workingDirectory} - {ex.Message}");
        }

        return false;

    }

    private record Header {

        public DateTime OrderDate { get; init; }
        public DateTime DueDate { get; init; }
        public string VendorName { get; init; } = string.Empty;
        public string CustomerName { get; init; } = string.Empty;
        public string OrderNumber { get; init; } = string.Empty;
        public string OrderName { get; init; } = string.Empty;
        public string ConnectorType { get; init; } = string.Empty;
        public string Construction { get; init; } = string.Empty;
        public string Units { get; init; } = string.Empty;
        public string SpecialInstructions { get; init; } = string.Empty;

        public static Header ReadFromSheet(Worksheet worksheet) {

            string[] noteSegments = new string[] {
                worksheet.Range["SpecialInstructions"].Value2?.ToString() ?? "",
                worksheet.Range["SpecialInstructions_2"].Value2?.ToString() ?? "",
                worksheet.Range["SpecialInstructions_3"].Value2?.ToString() ?? "",
                worksheet.Range["SpecialInstructions_4"].Value2?.ToString() ?? "",
                worksheet.Range["SpecialInstructions_5"].Value2?.ToString() ?? "",
                worksheet.Range["SpecialInstructions_6"].Value2?.ToString() ?? "",
                worksheet.Range["SpecialInstructions_7"].Value2?.ToString() ?? "",
                worksheet.Range["SpecialInstructions_8"].Value2?.ToString() ?? "",
                worksheet.Range["SpecialInstructions_9"].Value2?.ToString() ?? "",
                worksheet.Range["SpecialInstructions_10"].Value2?.ToString() ?? "",
                worksheet.Range["SpecialInstructions_11"].Value2?.ToString() ?? "",
                worksheet.Range["SpecialInstructions_12"].Value2?.ToString() ?? "",
            };

            return new () {
                OrderDate = DateTime.Today, // worksheet.Range["OrderDate"].Value2,
                DueDate = DateTime.Today, // worksheet.Range["DueDate"].Value2,
                VendorName = worksheet.Range["Vendor"].Value2.ToString(),
                CustomerName = worksheet.Range["CustomerName"].Value2.ToString(),
                OrderNumber = worksheet.Range["JobNumber"].Value2.ToString(),
                OrderName = worksheet.Range["JobName"].Value2.ToString(),
                ConnectorType = worksheet.Range["SelectedConnectionType"].Value2.ToString(),
                Construction = worksheet.Range["SelectedConstructionOption"].Value2.ToString(),
                Units = worksheet.Range["SelectedUnits"].Value2.ToString(),
                SpecialInstructions = string.Join("; ", noteSegments.Where(s => !string.IsNullOrWhiteSpace(s)))
            };

        }

    }

    private record LineItem {

        public int Number { get; init; }
        public string Note { get; init; } = string.Empty;
        public int Qty { get; init; }
        public double Height { get; init; }
        public double Width { get; init; }
        public double Depth { get; init; }
        public string Instructions { get; init; } = string.Empty;
        public decimal UnitPrice { get; init; }
        public string FrontBackColor { get; init; } = string.Empty;
        public double FrontBackThickness { get; init; }
        public bool FrontBackGrained { get; init; }
        public string SidesColor { get; init; } = string.Empty;
        public double SidesThickness { get; init; }
        public bool SidesGrained { get; init; }
        public string BottomColor { get; init; } = string.Empty;
        public double BottomThickness { get; init; }
        public bool BottomGrained { get; init; }

        public static bool DoesRowContainItem(Worksheet worksheet, int row) {

            var lineVal = worksheet.Range["BoxNumStart"].Offset[row].Value2;
            if (lineVal is null || lineVal.ToString() == "") {
                return false;
            }

            if (int.TryParse(lineVal?.ToString() ?? "", out int num)) {
                return num != 0;
            }

            return false;

        }

        public static LineItem ReadFromSheet(Worksheet worksheet, int row) {

            return new() {
                Number = (int) worksheet.Range["BoxNumStart"].Offset[row].Value2,
                Note = worksheet.Range["BoxNoteStart"].Offset[row].Value2,
                Qty = (int) worksheet.Range["BoxQtyStart"].Offset[row].Value2,
                Height = worksheet.Range["BoxHeightStart"].Offset[row].Value2,
                Width = worksheet.Range["BoxWidthStart"].Offset[row].Value2,
                Depth = worksheet.Range["BoxDepthStart"].Offset[row].Value2,
                Instructions = worksheet.Range["BoxInstructionsStart"].Offset[row].Value2,
                UnitPrice = (decimal) worksheet.Range["BoxUnitPriceStart"].Offset[row].Value2,
                FrontBackColor = worksheet.Range["FrontBackColorStart"].Offset[row].Value2,
                FrontBackThickness = worksheet.Range["FrontBackThicknessStart"].Offset[row].Value2,
                FrontBackGrained = worksheet.Range["FrontBackGrainedStart"].Offset[row].Value2 == "Yes",
                SidesColor = worksheet.Range["SidesColorStart"].Offset[row].Value2,
                SidesThickness = worksheet.Range["SidesThicknessStart"].Offset[row].Value2,
                SidesGrained = worksheet.Range["SidesGrainedStart"].Offset[row].Value2 == "Yes",
                BottomColor = worksheet.Range["BottomColorStart"].Offset[row].Value2,
                BottomThickness = worksheet.Range["BottomThicknessStart"].Offset[row].Value2,
                BottomGrained = worksheet.Range["BottomGrainedStart"].Offset[row].Value2 == "Yes"
            };

        }

        public DoweledDrawerBoxProduct CreateDoweledDrawerBoxProduct(bool useInches, bool machineThicknessForUMSlides, Dimension frontBackHeightAdjustment) {

            var id = Guid.NewGuid();
            var room = "";

            Func<double, Dimension> dimParse = useInches switch {
                true => Dimension.FromInches,
                false => Dimension.FromMillimeters,
            };

            Dimension height = dimParse(Height);
            Dimension width = dimParse(Width);
            Dimension depth = dimParse(Depth);
            Dimension frontBackThickness = dimParse(FrontBackThickness);
            Dimension sideThickness = dimParse(SidesThickness);
            Dimension bottomThickness = dimParse(BottomThickness);

            var frontBackMaterial = new DoweledDrawerBoxMaterial(FrontBackColor, frontBackThickness, FrontBackGrained);
            var sidesMaterial = new DoweledDrawerBoxMaterial(SidesColor, sideThickness, SidesGrained);
            var bottomMaterial = new DoweledDrawerBoxMaterial(BottomColor, bottomThickness, BottomGrained);

            return new DoweledDrawerBoxProduct(id, UnitPrice, Qty, room, Number, height, width, depth, frontBackMaterial, frontBackMaterial, sidesMaterial, bottomMaterial, machineThicknessForUMSlides, frontBackHeightAdjustment);

        }

    }

    private record CustomerInfo {

        public string Contact { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Line1 { get; init; } = string.Empty;
        public string Line2 { get; init; } = string.Empty;
        public string Line3 { get; init; } = string.Empty;
        public string City { get; init; } = string.Empty;
        public string State { get; init; } = string.Empty;
        public string Zip { get; init; } = string.Empty;

        public static CustomerInfo ReadFromSheet(Worksheet worksheet) {

            return new() {
                Contact = worksheet.Range["CustomerInfo_Contact"].Value2,
                Email = worksheet.Range["CustomerInfo_Email"].Value2,
                Line1 = worksheet.Range["CustomerInfo_Line1"].Value2,
                Line2 = worksheet.Range["CustomerInfo_Line2"].Value2,
                Line3 = worksheet.Range["CustomerInfo_Line3"].Value2,
                City = worksheet.Range["CustomerInfo_City"].Value2,
                State = worksheet.Range["CustomerInfo_State"].Value2,
                Zip = worksheet.Range["CustomerInfo_Zip"].Value2
            };

        }

    }

    private record UMSlideSpecs {

        public bool UMSlideMachining { get; set; }
        public double UMSlidesFromSide { get; set; }
        public bool UMSlideHook { get; set; }
        public double UMSlideHookFromEdge { get; set; }
        public double UMSlideHookFromBottom { get; set; }
        public double UMSlideHook5mmBoreDepth { get; set; }
        public double UMSlideHook8mmBoreDepth { get; set; }
        public bool UMSlideNotches { get; set; }
        public double UMSlideNotchHeight { get; set; }
        public double UMSlideNotchWidth { get; set; }
        public bool ResizeForUMSlides { get; set; }
        public double ResizeAmount { get; set; }
        public bool MachineFrontsForClips { get; set; }
        public double DistanceFromClipsToFace { get; set; }

        public static UMSlideSpecs ReadFromSheet(Worksheet worksheet) {

            return new() {
                UMSlideMachining = worksheet.Range["MachineSidesForSlides"].Value2 == "Yes",
                UMSlidesFromSide = worksheet.Range["SlidesFromSides"].Value2 as double? ?? 0,
                UMSlideHook = worksheet.Range["BoreForHook"].Value2 == "Yes",
                UMSlideHookFromEdge = worksheet.Range["HookFromEdge"].Value2 as double? ?? 0,
                UMSlideHookFromBottom = worksheet.Range["HookFromBottom"].Value2 as double? ?? 0,
                UMSlideHook5mmBoreDepth = worksheet.Range["Hook5mmDepth"].Value2 as double? ?? 0,
                UMSlideHook8mmBoreDepth = worksheet.Range["Hook8mmDepth"].Value2 as double? ?? 0,
                UMSlideNotches = worksheet.Range["CutNotches"].Value2 == "Yes",
                UMSlideNotchHeight = worksheet.Range["NotchHeight"].Value2 as double? ?? 0,
                UMSlideNotchWidth = worksheet.Range["NotchWidth"].Value2 as double? ?? 0,
                ResizeForUMSlides = worksheet.Range["ResizeForUMSlides"].Value2 == "Yes",
                ResizeAmount = worksheet.Range["ResizeAmountForUMSlides"].Value2 as double? ?? 0,
                MachineFrontsForClips = worksheet.Range["MachineFrontsForClips"].Value2 == "Yes",
                DistanceFromClipsToFace = worksheet.Range["ClipsFromFace"].Value2 as double? ?? 0
            };

        }

    }

    private record BottomSpecs {

        public double BottomDadoOversize { get; set; }
        public double BottomDadoDepth { get; set; }
        public double BottomHeight { get; set; }
        public double BottomDadoOvercut { get; set; }
        public double BottomDadoToolDiameter { get; set; }
        public double BottomSizeAdjustment { get; set; }
        public bool PreDrillBottoms { get; set; }
        public double MinPreDrillingSpace { get; set; }

        public static BottomSpecs ReadFromSheet(Worksheet worksheet) {

            return new() {
                BottomDadoOversize = worksheet.Range["BottomDadoOversize"].Value2 as double? ?? 0,
                BottomDadoDepth = worksheet.Range["BottomDadoDepth"].Value2 as double? ?? 0,
                BottomHeight = worksheet.Range["BottomHeight"].Value2 as double? ?? 0,
                BottomDadoOvercut = worksheet.Range["BottomDadoOvercut"].Value2 as double? ?? 0,
                BottomDadoToolDiameter = worksheet.Range["BottomDadoToolDiameter"].Value2 as double? ?? 0,
                BottomSizeAdjustment = worksheet.Range["BottomSizeAdjustment"].Value2 as double? ?? 0,
                PreDrillBottoms = worksheet.Range["PreDrillBottom"].Value2 == "Yes",
                MinPreDrillingSpace = worksheet.Range["MinPreDrillSpacing"].Value2 as double? ?? 0,
            };

        }

    }

    private record FrontDrillingSpecs {

        public bool PeanutSlotFronts { get; set; }
        public double PeanutSlotFrontOffSides { get; set; }
        public bool DrillFronts { get; set; }
        public double FrontDrillingOffSides { get; set; }

        public static FrontDrillingSpecs ReadFromSheet(Worksheet worksheet) {

            return new() {
                PeanutSlotFronts = worksheet.Range["SlotFront"].Value2 == "Yes",
                PeanutSlotFrontOffSides = worksheet.Range["FrontSlotsOffSides"].Value2 as double? ?? 0,
                DrillFronts = worksheet.Range["DrillFront"].Value2 == "Yes",
                FrontDrillingOffSides = worksheet.Range["FrontDrillingOffSides"].Value2 as double? ?? 0
            };

        }

    }

    private record ConstructionSpecs {

        public bool MachineFrontFromOutside { get; set; }
        public bool MachineBackFromOutside { get; set; }
        public double FrontBackDrop { get; set; }
        public double DrillingDepthAdjustment { get; set; }
        public bool FullHeightFront { get; set; }
        public bool FullHeightBack { get; set; }

        public static ConstructionSpecs ReadFromSheet(Worksheet worksheet) {

            return new() {
                MachineFrontFromOutside = worksheet.Range["MachineFrontFromOutside"].Value2 == "Yes",
                MachineBackFromOutside = worksheet.Range["MachineBackFromOutside"].Value2 == "Yes",
                FrontBackDrop = worksheet.Range["FrontBackAdj"].Value2 as double? ?? 0,
                DrillingDepthAdjustment = worksheet.Range["DrillDepthAdjustment"].Value2 as double? ?? 0,
                FullHeightFront = worksheet.Range["FullHeightFront"].Value2 == "Yes",
                FullHeightBack = worksheet.Range["FullHeightBack"].Value2 == "Yes"
            };

        }

    }

}
