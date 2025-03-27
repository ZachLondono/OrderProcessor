using Domain.Orders;
using Microsoft.Extensions.Logging;
using Microsoft.Office.Interop.Excel;
using Domain.Orders.Builders;
using Domain.Orders.Entities;
using Domain.Companies;
using System.Runtime.InteropServices;
using Domain.Orders.Components;
using Domain.Infrastructure.Bus;
using Domain.Services;

namespace OrderExporting.DoorOrderExport;

public class ExportDoorOrder {

    public record Command(Order Order, string TemplateFilePath, string OutputDirectory) : ICommand<DoorOrderExportResult>;

    public class Handler : CommandHandler<Command, DoorOrderExportResult> {

        private readonly ILogger<ExportDoorOrder> _logger;
        private readonly IFileReader _fileReader;
        private readonly ComponentBuilderFactory _factory;
        private readonly CompanyDirectory.GetCustomerByIdAsync _getCustomerByIdAsync;
        private readonly CompanyDirectory.GetVendorByIdAsync _getVendorByIdAsync;

        public Handler(ILogger<ExportDoorOrder> logger, IFileReader fileReader, ComponentBuilderFactory factory, CompanyDirectory.GetCustomerByIdAsync getCustomerByIdAsync, CompanyDirectory.GetVendorByIdAsync getVendorByIdAsync) {
            _logger = logger;
            _fileReader = fileReader;
            _factory = factory;
            _getCustomerByIdAsync = getCustomerByIdAsync;
            _getVendorByIdAsync = getVendorByIdAsync;
        }

        public override async Task<Response<DoorOrderExportResult>> Handle(Command command) {

            var doors = command.Order.Products
                                .OfType<IMDFDoorContainer>()
                                .SelectMany(p => p.GetDoors(() => _factory.CreateMDFDoorBuilder()))
                                .ToList();

            if (doors.Count == 0) {
                return new Domain.Infrastructure.Bus.Error() {
                    Title = "Cannot Export Door Order",
                    Details = "The provided order does not contain any doors"
                };
            }

            if (!File.Exists(command.TemplateFilePath)) {
                return new Domain.Infrastructure.Bus.Error() {
                    Title = "Cannot Export Door Order",
                    Details = "The provided door order template file path does not exist"
                };
            }

            if (!Directory.Exists(command.OutputDirectory)) {
                return new Domain.Infrastructure.Bus.Error() {
                    Title = "Cannot Export Door Order",
                    Details = "The provided output directory does not exist"
                };
            }

            try {

                var orders = await CreateDoorOrders(command.Order, doors);

                return GenerateOrderForms(orders, command.TemplateFilePath, command.OutputDirectory);

            } catch (Exception ex) {

                _logger.LogError(ex, "Exception thrown while filling door order");
                return new Domain.Infrastructure.Bus.Error() {
                    Title = "Cannot Export Door Order",
                    Details = $"An error occurred while trying to fill door order form - {ex.Message}"
                };

            }

        }

        private DoorOrderExportResult GenerateOrderForms(DoorOrder[] orders, string template, string outputDirectory) {

            Application app = new() {
                DisplayAlerts = false,
                Visible = false,
                ScreenUpdating = false
            };

            List<string> filesGenerated = new();

            var workbooks = app.Workbooks;
            bool wasExceptionThrown = false;
            foreach (var order in orders) {

                Workbook? workbook = null;

                try {

                    workbook = workbooks.Open(template, ReadOnly: true);

                    app.Calculation = XlCalculation.xlCalculationManual;

                    var worksheets = workbook.Worksheets;
                    Worksheet worksheet = (Worksheet)worksheets["MDF"];

                    order.WriteToWorksheet(worksheet);

                    _ = Marshal.ReleaseComObject(worksheet);
                    _ = Marshal.ReleaseComObject(worksheets);

                    string fileName = _fileReader.GetAvailableFileName(outputDirectory, $"{order.TrackingNumber} - {order.JobName} MDF DOORS", ".xlsm");
                    string finalPath = Path.GetFullPath(fileName);

                    workbook.SaveAs(finalPath);

                    filesGenerated.Add(finalPath);

                } catch (Exception ex) {

                    _logger.LogError(ex, "Exception thrown while filling door order group");
                    wasExceptionThrown = true;

                } finally {

                    if (workbook is not null) {
                        workbook.Close(SaveChanges: false);
                        _ = Marshal.ReleaseComObject(workbook);
                    }

                }

            }

            workbooks.Close();
            app?.Quit();

            _ = Marshal.ReleaseComObject(workbooks);
            if (app is not null) _ = Marshal.ReleaseComObject(app);

            // Clean up COM objects, calling these twice ensures it is fully cleaned up.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            string? error = wasExceptionThrown ? "An error occurred while trying to write one or more door orders" : null;
            return new(filesGenerated, error);

        }

        private async Task<DoorOrder[]> CreateDoorOrders(Order order, IEnumerable<MDFDoor> doors) {

            var customer = await _getCustomerByIdAsync(order.CustomerId);
            var vendor = await _getVendorByIdAsync(order.VendorId);

            return DoorOrder.FromOrder(order, doors, customer?.Name ?? "", vendor?.Name ?? "").ToArray();

        }

    }

}
