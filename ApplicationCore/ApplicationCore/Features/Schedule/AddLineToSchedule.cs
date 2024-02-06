using ApplicationCore.Shared.Settings;
using Microsoft.Extensions.Options;
using Microsoft.Office.Interop.Excel;
using Domain.Excel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ExcelApplication = Microsoft.Office.Interop.Excel.Application;
using Range = Microsoft.Office.Interop.Excel.Range;
using Domain.Infrastructure.Bus;

namespace ApplicationCore.Features.Schedule;

internal class AddLineToSchedule {

    public record Command(string JobNumber, string CustomerName, string JobName, string JC, string SC, string EC, string DD, DateTime BookingDate, DateTime ApprovalDate, DateTime RequestedDate) : ICommand;

    public class Handler : CommandHandler<Command> {

        private readonly ScheduleSettings _paths;

        public Handler(IOptions<ScheduleSettings> option) {
            _paths = option.Value;
        }

        public override async Task<Response> Handle(Command command) {

            try {

                await Task.Run(() => {

                    string workbookPath = _paths.ScheduleWorkbookPath;

                    var manager = new ExcelInstanceManager();

                    manager.OpenWorkbook(workbookPath);

                    if (manager.Workbook is not null) {

                        WriteLineToWorkbook(command, manager.Workbook);

                        manager.Close();

                    }

                });

            } catch (Exception ex) {

                return new Domain.Infrastructure.Bus.Error() {
                    Title = "Error Occurred While Writing to Schedule",
                    Details = ex.Message
                };

            }

            return Response.Success();

        }

        private static void WriteLineToWorkbook(Command command, Workbook workbook) {

            Worksheet main = workbook.Sheets["Main"];
            Range jobNumberCol = main.Range["A1"];
            while (true) {

                jobNumberCol = jobNumberCol.Offset[1];

                if (jobNumberCol.Value2 is null || string.IsNullOrWhiteSpace(jobNumberCol.Value2?.ToString())) break;

            }

            jobNumberCol.Value2 = command.JobNumber;
            jobNumberCol.Offset[0, 1].Value2 = command.CustomerName;
            jobNumberCol.Offset[0, 2].Value2 = command.JobName;
            jobNumberCol.Offset[0, 3].Value2 = command.JC;
            jobNumberCol.Offset[0, 4].Value2 = command.SC;
            jobNumberCol.Offset[0, 5].Value2 = command.EC;
            jobNumberCol.Offset[0, 6].Value2 = command.DD;
            jobNumberCol.Offset[0, 7].Value2 = command.BookingDate.ToString("d");
            jobNumberCol.Offset[0, 8].Value2 = command.ApprovalDate.ToString("d");
            jobNumberCol.Offset[0, 9].Value2 = command.RequestedDate.ToString("d");

            Marshal.ReleaseComObject(jobNumberCol);
            Marshal.ReleaseComObject(main);

        }

        private class ExcelInstanceManager {

            private bool _wasCreated = false;
            public ExcelApplication? App { get; private set; }
            public Workbook? Workbook { get; private set; }

            public void OpenWorkbook(string fullPath) {

                var allProcesses = Process.GetProcesses();
                List<ExcelApplication> excelApps = new();

                foreach (var process in allProcesses) {

                    nint winHandle = process.MainWindowHandle;

                    var retriever = new ExcelApplicationRetriever((int)winHandle);

                    if (retriever.xl is not null) {
                        excelApps.Add(retriever.xl);
                    }

                }

                foreach (var excelApp in excelApps) {

                    foreach (Workbook workbook in excelApp.Workbooks) {
                        if (0 == string.Compare(Path.GetFullPath(workbook.FullName),
                                        Path.GetFullPath(fullPath),
                                        StringComparison.InvariantCultureIgnoreCase)) {
                            App = excelApp;
                            Workbook = workbook;
                            _wasCreated = false;
                            return;
                        }
                    }

                }

                App = new ExcelApplication() {
                    Visible = false
                };

                Workbook = App.Workbooks.Open(fullPath);

                _wasCreated = true;

                return;

            }

            public void Close() {

                if (!_wasCreated) return;

                if (Workbook is not null) {
                    Workbook.Close(SaveChanges: true);
                }

                if (App is not null) {
                    App.Quit();
                    Marshal.ReleaseComObject(App);
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();

            }

        }

    }

}

