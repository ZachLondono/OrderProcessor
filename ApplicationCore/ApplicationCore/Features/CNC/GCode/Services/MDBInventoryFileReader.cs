using ApplicationCore.Features.CNC.GCode.Domain;
using ApplicationCore.Features.CNC.GCode.Domain.Inventory;
using ApplicationCore.Shared;
using ApplicationCore.Shared.Domain;
using Dapper;

namespace ApplicationCore.Features.CNC.GCode.Services;

internal class MDBInventoryFileReader : IInventoryFileReader {

    private readonly IAccessDBConnectionFactory _factory;

    public MDBInventoryFileReader(IAccessDBConnectionFactory factory) {
        _factory = factory;
	}

	public async Task<IEnumerable<InventorySheetStock>> GetAvailableInventoryAsync(string filePath) {

		using var connection = _factory.CreateConnection(filePath);

        const string query = @"SELECT
								[SheetStock], [Thickness], [Units], [Graining], [Length], [Width], [Priority]
								FROM [CADCode Inventory File];";

        var data = await connection.QueryAsync<InventoryItemModel>(query);


        return data.Where(i => i.Width != 0 && i.Length != 0)
            .GroupBy(i => i.SheetStock)
            .Select(group => {

                var item = group.First();

                return new InventorySheetStock() {
                    Name = group.Key,
                    IsGrained = item.Graining == 1,
                    Thickness = Dimension.FromMillimeters(item.Thickness / (item.Units == 0 ? 25.4 : 1)),
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