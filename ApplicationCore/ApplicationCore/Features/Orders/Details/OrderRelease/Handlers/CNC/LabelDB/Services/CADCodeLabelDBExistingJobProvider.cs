using ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.LabelDB.Contracts;
using ApplicationCore.Features.Shared.Services;
using Dapper;
using System.Data;
using System.Data.OleDb;

namespace ApplicationCore.Features.Orders.Details.OrderRelease.Handlers.CNC.LabelDB.Services;

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

        connection.Open();
        DataTable? schema = await connection.GetSchemaAsync("COLUMNS");
        connection.Close();

        string productIdSubQuery = GetProductIdSubQuery(jobName, schema);
        string productClassSubQuery = GetPartClassSubQuery(jobName, schema);

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
                (Face6Part = 0) AS HasFace6,
                {productIdSubQuery},
                {productClassSubQuery}
		    From [Label Sequence:{jobName}]");

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

    private static string GetProductIdSubQuery(string jobName, DataTable? schema) {

        bool containsId = schema?.Select($"TABLE_NAME='{jobName}' AND COLUMN_NAME='ProductId'").Length > 0;

        if (containsId) {

            return $"(SELECT TOP 1 ProductId FROM [{jobName}] WHERE cStr([{jobName}].[GlobalId]) = cStr([Label Sequence:{jobName}].[GlobalId])) As ProductId";

        }

        return "\"0\" As ProductId";

    }

    private static string GetPartClassSubQuery(string jobName, DataTable? schema) {

        bool containsId = schema?.Select($"TABLE_NAME='{jobName}' AND COLUMN_NAME='PartClass'").Length > 0;

        if (containsId) {

            return $"(SELECT TOP 1 PartClass FROM [{jobName}] WHERE cStr([{jobName}].[GlobalId]) = cStr([Label Sequence:{jobName}].[GlobalId])) As PartClass";

        }

        return "\"Unknown\" As PartClass";

    }

}