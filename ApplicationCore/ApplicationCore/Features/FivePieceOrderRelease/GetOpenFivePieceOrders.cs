using Domain.Excel;
using Domain.Infrastructure.Bus;
using Microsoft.Extensions.Logging;
using Microsoft.Office.Interop.Excel;
using System.Diagnostics;
using ExcelApplication = Microsoft.Office.Interop.Excel.Application;

namespace ApplicationCore.Features.FivePieceOrderRelease;

public static class GetOpenFivePieceOrders {

    public record Query() : IQuery<IEnumerable<FivePieceOrderFile>>;

    public class Handler(ILogger<Handler> logger) : QueryHandler<Query, IEnumerable<FivePieceOrderFile>> {

        private readonly ILogger<Handler> _logger = logger;

        public override async Task<Response<IEnumerable<FivePieceOrderFile>>> Handle(Query query) {

            try {

                List<FivePieceOrderFile> orders = await Task.Run(LoadOpenOrders);
                return Response<IEnumerable<FivePieceOrderFile>>.Success(orders);

            } catch (Exception ex) {
                _logger.LogError(ex, "Exception thrown while trying to list all open door orders");
                return Response<IEnumerable<FivePieceOrderFile>>.Error(new() {
                    Title = "Failed to Get Open Orders",
                    Details = ex.Message
                });
            }

        }

        private List<FivePieceOrderFile> LoadOpenOrders() {

            List<FivePieceOrderFile> orders = [];

            var allProcesses = Process.GetProcesses();

            List<ExcelApplication> excelApps = [];

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

                    try {

                        var sheet = worksheets.OfType<Worksheet>().FirstOrDefault(ws => ws.Name == "Order");

                        if (sheet is null) continue;

                        string filePath = workbook.FullName;
                        var companyName = sheet.Range["Company"].Value2;
                        var trackingNumber = sheet.Range["TrackingNumber"].Value2;
                        var jobName = sheet.Range["JobName"].Value2;

                        orders.Add(new(filePath, companyName, trackingNumber, jobName));

                    } catch (Exception ex) {
                        _logger.LogWarning(ex, "Exception thrown while trying to read order info");
                    }

                }
            }

            return orders;

        }

    }

}
