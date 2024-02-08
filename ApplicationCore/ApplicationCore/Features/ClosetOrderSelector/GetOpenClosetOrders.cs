using Microsoft.Extensions.Logging;
using Microsoft.Office.Interop.Excel;
using System.Diagnostics;
using Domain.Excel;
using ExcelApplication = Microsoft.Office.Interop.Excel.Application;
using Domain.Infrastructure.Bus;

namespace ApplicationCore.Features.ClosetOrderSelector;

public class GetOpenClosetOrders {

    public record Query() : IQuery<IEnumerable<ClosetOrder>>;

    public class Handler : QueryHandler<Query, IEnumerable<ClosetOrder>> {

        private readonly ILogger<Handler> _logger;

        public Handler(ILogger<Handler> logger) {
            _logger = logger;
        }

        public override Task<Response<IEnumerable<ClosetOrder>>> Handle(Query query) {

            List<ClosetOrder> closetOrders = new();
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
                            if (worksheet.Name != "Cover") continue;

                            try {

                                string[] orderNumParts = (worksheet.Range["OrderNum"].Value2.ToString()).Split(' ', 2, StringSplitOptions.None);
                                string customerName = worksheet.Range["CustomerName"].Value2.ToString();
                                string jobNumber = orderNumParts[0];
                                string jobName = worksheet.Range["JobName"].Value2.ToString();
                                DateTime orderDate = ReadDateTimeFromWorkbook(worksheet, "E4");
                                DateTime dueDate = ReadDateTimeFromWorkbook(worksheet, "E5");
                                string reportFilePath = @$"Y:\CADCode\Reports\{jobNumber} {jobName}.xml"; // TODO: Get this directory from a settings file
                                string directory = workbook.Path;
                                string filePath = workbook.FullName;

                                closetOrders.Add(new(customerName, jobName, jobNumber, orderDate, dueDate, reportFilePath, filePath, directory));

                            } catch (Exception ex) {
                                _logger.LogWarning(ex, "Exception thrown while trying to read order info from door order Cover tab");
                            }
                        }

                    }
                }

            } catch (Exception ex) {
                _logger.LogError(ex, "Exception thrown while trying to list all open door orders");
                return Task.FromResult(Response<IEnumerable<ClosetOrder>>.Error(new() {
                    Title = "Failed to Get Open Orders",
                    Details = ex.Message
                }));
            }

            return Task.FromResult(Response<IEnumerable<ClosetOrder>>.Success(closetOrders));

        }

        private static DateTime ReadDateTimeFromWorkbook(Worksheet sheet, string rangeName) {

            try {

                var rng = sheet.Range[rangeName];

                if (rng.Value2 is double oaDate) {
                    return DateTime.FromOADate(oaDate);
                }

                return DateTime.Today;

            } catch {

                return DateTime.Today;

            }

        }

    }

}
