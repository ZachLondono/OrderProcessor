using ApplicationCore.Features.CNC.Services.Domain.Inventory;
using Dapper;

namespace ApplicationCore.Features.CNC.Services;

public class MDBInventoryService : IInventoryService {

	private readonly string _filePath;
	private readonly ICADCodeInventoryDataBaseConnectionFactory _factory;

	public MDBInventoryService(string filePath, ICADCodeInventoryDataBaseConnectionFactory factory) {
		_filePath = filePath;
		_factory = factory;
	}

	public async Task<IEnumerable<InventoryItem>> GetInventory() {

		using var connection = _factory.CreateConnection(_filePath);

		const string query = @"SELECT
								[SheetStock], [Thickness], [Units], [Graining], [Length], [Width], [Priority]
								FROM [CADCode Inventory File];";

		var data = await connection.QueryAsync<InventoryItemModel>(query);


		return data.Where(i => i.Width != 0 && i.Length != 0)
			.GroupBy(i => i.SheetStock)
			.Select(group => {

				var item = group.First();

				return new InventoryItem() {
					Name = group.Key,
					IsGrained = item.Graining == 1,
					Thickness = item.Thickness / (item.Units == 0 ? 25.4 : 1),
					Sizes = group.Select(i => new InventorySize() {
						Width = i.Width / (item.Units == 0 ? 25.4 : 1),
						Length = i.Length / (item.Units == 0 ? 25.4 : 1),
						Priority = i.Priority
					})
				};

			});
		

	}

	class InventoryItemModel {

		public string SheetStock { get; set; } = string.Empty;

		public double Thickness { get; set; }

		public int Units { get; set; }

		public int Graining { get; set; }

		public double Length { get; set; }

		public double Width { get; set; }

		public int Priority { get; set; }

	}

}