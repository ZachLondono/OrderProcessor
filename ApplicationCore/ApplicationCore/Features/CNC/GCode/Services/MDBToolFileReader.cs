using ApplicationCore.Features.CNC.GCode.Domain;
using ApplicationCore.Features.CNC.Shared;
using ApplicationCore.Shared;
using ApplicationCore.Shared.Domain;
using Dapper;

namespace ApplicationCore.Features.CNC.GCode.Services;

internal class MDBToolFileReader : IToolFileReader {

	private readonly IAccessDBConnectionFactory _factory;

	public MDBToolFileReader(IAccessDBConnectionFactory factory) {
		_factory = factory;
	}

	public async Task<IEnumerable<Tool>> GetAvailableToolsAsync(string filePath) {

		using var connection = _factory.CreateConnection(filePath);

		const string query = @"SELECT [Name], [Diameter], [Feed Speed] As FeedSpeed, [Entry Speed] As EntrySpeed, [Rotation Speed] As SpindleSpeed FROM [Tools];";

		var data = await connection.QueryAsync<ToolItemModel>(query);

		return data.Select(d => new Tool() {
			Name = d.Name,
			Diameter = Dimension.FromMillimeters(d.Diameter),
			CornerSpeed = Speed.FromMillimetersPerSecond(d.CornerSpeed),
			EntrySpeed = Speed.FromMillimetersPerSecond(d.EntrySpeed),
			FeedSpeed = Speed.FromMillimetersPerSecond(d.FeedSpeed),
			SpindleSpeed = Speed.FromMillimetersPerSecond(d.SpindleSpeed)
		});

	}


	class ToolItemModel {
		public string Name { get; set; } = string.Empty;
		public double Diameter { get; set; }
		public double CornerSpeed { get; set; }
		public double EntrySpeed { get; set; }
		public double FeedSpeed { get; set; }
		public double SpindleSpeed { get; set; }
	}

}