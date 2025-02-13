using Microsoft.Extensions.Logging;
using Microsoft.Office.Interop.Excel;
using System.Diagnostics;
using Domain.Excel;
using ExcelApplication = Microsoft.Office.Interop.Excel.Application;
using Domain.Infrastructure.Bus;
using Error = Domain.Infrastructure.Bus.Error;
using System.Runtime.InteropServices;
using ApplicationCore.Shared.Settings;
using Microsoft.Extensions.Options;

namespace ApplicationCore.Features.ClosetOrders.ClosetOrderSelector;

public class GetOpenClosetOrders {

    public record Query() : IQuery<IEnumerable<ClosetOrder>>;

    public class Handler : QueryHandler<Query, IEnumerable<ClosetOrder>> {

        private readonly ILogger<Handler> _logger;
        private readonly ClosetReleaseSettings _settings;

        public Handler(ILogger<Handler> logger, IOptions<ClosetReleaseSettings> settings) {
            _logger = logger;
            _settings = settings.Value;
        }

        public override async Task<Response<IEnumerable<ClosetOrder>>> Handle(Query query) {

            try {

                return await Task.Run(LoadOpenClosetOrders);

            } catch (COMException ex) {

                _logger.LogError(ex, "Exception thrown while trying to list all open closet orders");

                if (ex.Message.Contains("RPC_E_SERVERCALL_RETRYLATER", StringComparison.InvariantCultureIgnoreCase)) {

                    return new Error() {
                        Title = "Excel is Being Edited",
                        Details = "Please stop editing cells in Excel and try again."
                    };

                } else {

                    return new Error() {
                        Title = "Failed to Get Open Orders",
                        Details = ex.Message
                    };

                }

            } catch (Exception ex) {

                _logger.LogError(ex, "Exception thrown while trying to list all open closet orders");
                return new Error() {
                    Title = "Failed to Get Open Orders",
                    Details = ex.Message
                };

            }

        }

        private Response<IEnumerable<ClosetOrder>> LoadOpenClosetOrders() {

            List<ClosetOrder> closetOrders = [];

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

                    try {

                        var coverSheet = worksheets.OfType<Worksheet>().FirstOrDefault(ws => ws.Name == "Cover");

                        if (coverSheet is null) continue;

                        string[] orderNumParts = (coverSheet.Range["OrderNum"].Value2.ToString()).Split(' ', 2, StringSplitOptions.None);
                        string customerName = coverSheet.Range["CustomerName"].Value2.ToString();
                        string jobNumber = orderNumParts[0];
                        string jobName = coverSheet.Range["JobName"].Value2.ToString();
                        DateTime orderDate = ReadDateTimeFromWorkbook(coverSheet, "E4");
                        DateTime dueDate = ReadDateTimeFromWorkbook(coverSheet, "E5");
                        string reportFilePath = Path.Combine(_settings.WSXMLReportDirectory, $"{jobNumber} {jobName}.xml");
                        string directory = workbook.Path;
                        string filePath = workbook.FullName;

                        bool containsDovetail = false;
                        var dovetailSheet = worksheets.OfType<Worksheet>().FirstOrDefault(ws => ws.Name == "Dovetail");
                        if (dovetailSheet is not null) {
                            string? firstQty = dovetailSheet.Range["B17"]?.Value2?.ToString() ?? null;
                            containsDovetail = !string.IsNullOrWhiteSpace(firstQty);
                        }

                        bool containsMDF = false;
                        var mdfSheet = worksheets.OfType<Worksheet>().FirstOrDefault(ws => ws.Name == "MDF Fronts");
                        if (mdfSheet is not null) {
                            string? firstQty = mdfSheet.Range["B6"]?.Value2?.ToString() ?? null;
                            containsMDF = !string.IsNullOrWhiteSpace(firstQty);
                        }

                        bool containsOther = false;
                        var otherSheet = worksheets.OfType<Worksheet>().FirstOrDefault(ws => ws.Name == "Other");
                        if (otherSheet is not null) {
                            string? firstQty = otherSheet.Range["B2"]?.Value2?.ToString() ?? null;
                            containsOther = !string.IsNullOrWhiteSpace(firstQty);
                        }

                        closetOrders.Add(new(customerName, jobName, jobNumber, orderDate, dueDate, containsMDF, containsDovetail, containsOther, reportFilePath, filePath, directory, app.IsSandboxed));

                    } catch (Exception ex) {
                        _logger.LogWarning(ex, "Exception thrown while trying to read order info from door order Cover tab");
                    }

                }
            }

            return closetOrders;

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
