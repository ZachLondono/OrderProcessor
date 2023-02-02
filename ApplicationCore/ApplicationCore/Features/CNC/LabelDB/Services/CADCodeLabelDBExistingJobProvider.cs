using ApplicationCore.Features.CNC.LabelDB.Contracts;
using ApplicationCore.Features.Shared;
using Dapper;

namespace ApplicationCore.Features.CNC.LabelDB.Services;

internal class CADCodeLabelDBExistingJobProvider : IExistingJobProvider {

    private readonly MachineNameProvider _machineNameProvider;
    private readonly IAccessDBConnectionFactory _connFactory;

    public CADCodeLabelDBExistingJobProvider(MachineNameProvider machineNameProvider, IAccessDBConnectionFactory connFactory) {
        _machineNameProvider = machineNameProvider;
        _connFactory = connFactory;
    }

    public async Task<ExistingJob> LoadExistingJobAsync(string source, string jobName) {

        string machineName = await _machineNameProvider.GetMachineNameAsync(source, jobName);

        using var connection = _connFactory.CreateConnection(source);

        var inventory = await connection.QueryAsync<UsedInventory>($"SELECT [Sheets Used] As Qty, Description AS Name, Width, Length, Thickness, Graining As Grained FROM [Inventory:{jobName}];");

        var parts = await connection.QueryAsync<ManufacturedPart>(
            @$"SELECT
			    (SELECT TOP 1 [Face5FileName] FROM [Parts:{jobName}] WHERE cStr([Parts:{jobName}].[GlobalId]) = cStr([Label Sequence:{jobName}].[GlobalId])) As Name,
			    (SELECT TOP 1 [Description1] FROM [Parts:{jobName}] WHERE cStr([Parts:{jobName}].[GlobalId]) = cStr([Label Sequence:{jobName}].[GlobalId])) As Description,
			    Pattern AS PatternNumber,
			    Width,
			    Length,
			    XDimension As InsertX,
			    YDimension As InsertY,
                (SELECT TOP 1 [Cabinet Number] FROM [{jobName}] WHERE cStr([{jobName}].[GlobalId]) = cStr([Label Sequence:{jobName}].[GlobalId])) As ProductNumber,
		    From [Label Sequence:{jobName}];");

        var patterns = await connection.QueryAsync<Pattern>($"SELECT [PatternFilename] AS Name, [PatternFilename] AS ImagePath, Material AS MaterialName, [Panel Width] AS MaterialWidth, [Panel Length] AS MaterialLength, [Thickness] As MaterialThickness  FROM [Label Sequence:{jobName}]");
        patterns = patterns.Distinct().ToList();

        return new ExistingJob(
            Name: jobName,
            MachineName: machineName,
            Inventory: inventory,
            Patterns: patterns,
            Parts: parts
        );

    }


}