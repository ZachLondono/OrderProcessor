using Domain.Orders.Enums;
using Domain.Orders.Entities.Products.Closets;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;

namespace OrderLoading.ClosetProCSVCutList.Products.Verticals;

public class DividerVerticalPanel : IClosetProProduct {

	public required int Qty { get; set; }
	public required string Color { get; init; }
	public required string EdgeBandingColor { get; init; }
	public required string Room { get; init; }
	public required decimal UnitPrice { get; init; }
	public required int PartNumber { get; init; }

	public required Dimension Height { get; init; }
	public required Dimension Depth { get; init; }
	public required VerticalPanelDrilling Drilling { get; init; }

	public IProduct ToProduct() {

		var vertDrillingType = VerticalDividerPanelEndDrillingType.SingleCams;
		string vertSku = $"PCDV{ClosetProPartMapper.GetDividerPanelSuffix(vertDrillingType)}";

		var material = new ClosetMaterial(Color, ClosetMaterialCore.ParticleBoard);

		return new ClosetPart(Guid.NewGuid(),
							  Qty,
							  UnitPrice,
							  PartNumber,
							  Room,
							  vertSku,
							  Depth,
							  Height,
							  material,
							  null,
							  EdgeBandingColor,
							  "",
							  new Dictionary<string, string>());

	}

}