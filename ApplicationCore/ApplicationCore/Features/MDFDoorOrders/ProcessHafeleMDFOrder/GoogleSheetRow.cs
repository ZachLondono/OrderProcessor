using System.Diagnostics;

namespace ApplicationCore.Features.MDFDoorOrders.ProcessHafeleMDFOrder;

public class GoogleSheetRow {

    public string FileDate { get; set; } = string.Empty;
    public string HafelePO { get; set; } = string.Empty;
    public string ProjectNumber { get; set; } = string.Empty;
    public string ConfigNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPO { get; set; } = string.Empty;
    public string TotalItemCount { get; set; } = string.Empty;
    public string ShipDate { get; set; } = string.Empty;
    public string InvoiceAmount { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;

    public async Task PostData() {

        ProcessStartInfo startInfo = new() {
            CreateNoWindow = true,
            UseShellExecute = false,
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = "R:\\DB ORDERS\\GoogleSheetsExe\\publish\\GoogleSheetsUpdater.exe",
            Arguments = $"hafele \"{FileDate}\" \"{HafelePO}\" \"{ProjectNumber}\" \"{ConfigNumber}\" \"{CustomerName}\" \"{CustomerPO}\" \"{TotalItemCount}\" \"{ShipDate}\" \"{InvoiceAmount}\" \"{TrackingNumber}\""
        };

        try {

            using Process? process = Process.Start(startInfo);

            if (process is null) {
                return;
            }

            await process.WaitForExitAsync();

        } catch (Exception e) {
            throw new Exception("Error while tracking order on google sheet", e);
            throw;
        }

    }

}
