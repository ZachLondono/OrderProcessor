using ApplicationCore.Features.CNC.LabelDB;
using Dapper;

namespace ApplicationCore.Features.CNC.Services;

internal class MachineNameProvider {

    private readonly ICADCodeLabelDataBaseConnectionFactory _connFactory;

    public MachineNameProvider(ICADCodeLabelDataBaseConnectionFactory connFactory) {
        _connFactory = connFactory;
    }

    public async Task<string> GetMachineNameAsync(string filePath, string jobName) {

        using var connection = _connFactory.CreateConnection(filePath);

        var patternBarcode = await connection.QuerySingleAsync<string>($"SELECT TOP 1 [Pattern Barcode] FROM [{jobName}] WHERE [Pattern Barcode] IS NOT NULL;");

        return patternBarcode.Contains(".cnc") ? "Andi Stratos" : "Omnitech";

    }

}