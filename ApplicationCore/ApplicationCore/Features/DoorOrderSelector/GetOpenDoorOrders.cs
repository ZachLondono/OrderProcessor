using ApplicationCore.Infrastructure.Bus;
using Microsoft.Office.Interop.Excel;
using ExcelApplication = Microsoft.Office.Interop.Excel.Application;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using ApplicationCore.Shared.Services;

namespace ApplicationCore.Features.OpenDoorOrders;

public class GetOpenDoorOrders {

    public record Query() : IQuery<IEnumerable<DoorOrder>>;

    public class Handler : QueryHandler<Query, IEnumerable<DoorOrder>> {

        private readonly ILogger<Handler> _logger;

        public Handler(ILogger<Handler> logger) {
            _logger = logger;
        }

        public override Task<Response<IEnumerable<DoorOrder>>> Handle(Query query) {

            List<DoorOrder> doorOrders = new();
            try {

                var allProcesses = Process.GetProcesses();
                List<ExcelApplication> excelApps = new();

                foreach (var process in allProcesses) {

                    nint winHandle = process.MainWindowHandle;

                    var retriever = new ExcelApplicationRetriever((int)winHandle);

                    if (retriever.xl is not null) {
                        excelApps.Add(retriever.xl);
                    }

                }

                foreach (var app in excelApps) {
                    Workbooks workbooks = app.Workbooks;
                    foreach (Workbook workbook in workbooks) {
                        Sheets worksheets = workbook.Worksheets;
                        foreach (Worksheet worksheet in worksheets) {
                            if (worksheet.Name != "MDF") continue;

                            try {

                                string customerName = worksheet.Range["Company"].Value2.ToString();
                                string vendorName = worksheet.Range["Vendor"].Value2.ToString();
                                string jobName = worksheet.Range["JobName"].Value2.ToString();
                                string jobNumber = worksheet.Range["JobNumber"].Value2.ToString();
                                string reportFilePath = @$"Y:\CADCode\Reports\{jobNumber} - {jobName}.xml"; // TODO: Get this directory from a settings file
                                string directory = workbook.Path;

                                doorOrders.Add(new(customerName, vendorName, jobName, jobNumber, reportFilePath, directory));

                            } catch (Exception ex) {
                                _logger.LogWarning(ex, "Exception thrown while trying to read order info from door order MDF tab");
                            }
                        }

                    }
                }

            } catch (Exception ex) {
                _logger.LogError(ex, "Exception thrown while trying to list all open door orders");
                return Task.FromResult(Response<IEnumerable<DoorOrder>>.Error(new() {
                    Title = "Failed to Get Open Orders",
                    Details = ex.Message
                }));
            }

            return Task.FromResult(Response<IEnumerable<DoorOrder>>.Success(doorOrders));

        }

    }

}
