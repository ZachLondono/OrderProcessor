using ApplicationCore.Features.CADCode.Contracts.ProgramRelease;
using ApplicationCore.Features.CADCode.Services.Domain;
using Dapper;

namespace ApplicationCore.Features.CADCode.Services;

internal class CADCodeLabelDBExistingJobProvider : IExistingJobProvider {

	private readonly MachineNameProvider _machineNameProvider;
	private readonly ICADCodeLabelDataBaseConnectionStringFactory _connFactory;
	private readonly ICNCConfigurationProvider _cncConfigProvider;

	public CADCodeLabelDBExistingJobProvider(MachineNameProvider machineNameProvider, ICADCodeLabelDataBaseConnectionStringFactory connFactory, ICNCConfigurationProvider cncConfigProvider) {
		_machineNameProvider = machineNameProvider;
		_connFactory = connFactory;
		_cncConfigProvider = cncConfigProvider;
	}

	public async Task<ReleasedJob> LoadExistingJobAsync(string source, string jobName) {

		string machineName = await _machineNameProvider.GetMachineNameAsync(source, jobName);
		var machineConfig = _cncConfigProvider.GetConfigurations().FirstOrDefault(c => c.MachineName.Equals(machineName));
		TableOrientation orientation = TableOrientation.Standard;
		if (machineConfig is not null) {
			orientation = machineConfig.Orientation;
		} else {
			// TODO: error loading machine config
		}

		using var connection = _connFactory.CreateConnection(source);

		var inventory = await connection.QueryAsync<UsedInventory>($"SELECT [Sheets Used] As Qty, Description AS Name, Width, Length, Thickness, Graining As Grained FROM [Inventory:{jobName}];");

		var manufParts = await connection.QueryAsync<ManufacturedPart>(
			@$"SELECT
			(SELECT TOP 1 [Face5FileName] FROM [Parts:{jobName}] WHERE [Parts:{jobName}].[GlobalId] = [Label Sequence:{jobName}].[GlobalId]) As Name,
			(SELECT TOP 1 [Description1] FROM [Parts:{jobName}] WHERE [Parts:{jobName}].[GlobalId] = [Label Sequence:{jobName}].[GlobalId]) As Description,
			Pattern AS PatternNumber,
			Width,
			Length,
			XDimension As InsertX,
			YDimension As InsertY
		From [Label Sequence:{jobName}];"
		);

		var patterns = await connection.QueryAsync<Pattern>($"SELECT [PatternFilename] AS Name, [PatternFilename] AS ImagePath, Material AS MaterialName, [Panel Width] AS MaterialWidth, [Panel Length] AS MaterialLength, [Thickness] As MaterialThickness  FROM [Label Sequence:{jobName}]");
		patterns = patterns.Distinct().ToList();

		List<ReleasedProgram> releasedPrograms = new();
		int index = 1;
		foreach (var pattern in patterns) {

			var parts = manufParts.Where(p => p.PatternNumber == index).Select(p => new NestedPart() {
				Name = p.Name,
				ImagePath = "C:\\Users\\Zachary Londono\\Desktop\\CC Output\\Pix\\002471001.wmf",
				Width = (double)p.Width,
				Length = (double)p.Length,
				Description = p.Description,
				Center = new(p.InsertX, p.InsertY)
			}).ToList();

			var program = new ReleasedProgram() {
				Name = pattern.Name,
				ImagePath = "C:\\Users\\Zachary Londono\\Desktop\\CC Output\\Pix\\002471001.wmf",
				Material = new() {
					Name = pattern.MaterialName,
					Width = (double)pattern.MaterialWidth,
					Length = (double)pattern.MaterialLength,
					Thickness = (double)pattern.MaterialThickness,
					IsGrained = (inventory.Where(i => i.Name.Equals(pattern.MaterialName)).First().Grained.Equals("true") ? true : false),
					Yield = 0
				},
				Parts = parts
			};

			releasedPrograms.Add(program);

			index++;

		}

		var machineRelease = new MachineRelease() {
			MachineName = machineName,
			Programs = releasedPrograms,
			MachineTableOrientation = orientation
		};

		var job = new ReleasedJob() {
			JobName = jobName,
			Releases = new List<MachineRelease>() { machineRelease }
		};

		return job;

	}

	public record Job(int Id, string Name, string Created);
	public record Part(int Qty, string Name, float Width, float Length);
	public record UsedInventory(int Qty, string Name, string Width, string Length, string Thickness, string Grained);
	public record Pattern(string Name, string ImagePath, string MaterialName, float MaterialWidth, float MaterialLength, float MaterialThickness);
	public record ManufacturedPart(string Name, string Description, int PatternNumber, float Width, float Length, float InsertX, float InsertY);


}
