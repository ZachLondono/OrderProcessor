using System.Diagnostics;

namespace ApplicationCore.Features.ExcelTemplates.Domain;

internal class ProcessExcelPrinter : IExcelPrinter {

    public async Task PrintFile(string filePath) {
        var p = Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true, Verb = "print" });
        if (p is null) return; // TODO: notify that file could not be printed
        await p.WaitForExitAsync();
    }

}