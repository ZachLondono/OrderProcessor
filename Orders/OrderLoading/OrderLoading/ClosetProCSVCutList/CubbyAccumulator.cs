using OrderLoading.ClosetProCSVCutList.CSVModels;
using OrderLoading.ClosetProCSVCutList.Products;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.ValueObjects;
using OrderLoading.ClosetProCSVCutList.Products.Shelves;
using OrderLoading.ClosetProCSVCutList.Products.Verticals;

namespace OrderLoading.ClosetProCSVCutList;

public class CubbyAccumulator {

	private readonly List<Part> _verticalPanels = new();
	private readonly List<Part> _horizontalPanels = new();
	private Part? _topShelf = null;
	private Part? _bottomShelf = null;

	public void AddVerticalPanel(Part part) {
		_verticalPanels.Add(part);
	}

	public void AddHorizontalPanel(Part part) {
		_horizontalPanels.Add(part);
	}

	public void AddTopShelf(Part part) {
		_topShelf = part;
	}

	public void AddBottomShelf(Part part) {
		_bottomShelf = part;
	}

	public Cubby CreateCubby(RoomNamingStrategy strategy) {

		if (_verticalPanels.Count == 0) {
			throw new InvalidOperationException("Missing vertical cubby dividers");
		}

		if (_topShelf is null) {
			throw new InvalidOperationException("Missing cubby top shelf");
		}

		if (_bottomShelf is null) {
			throw new InvalidOperationException("Missing cubby bottom shelf");
		}

		if (_topShelf.Depth != _bottomShelf.Depth
			|| _horizontalPanels.Any(p => p.Depth != _topShelf.Depth)
			|| _verticalPanels.Any(p => p.Depth != _topShelf.Depth)) {
			throw new InvalidOperationException("Panels in cubby do not have matching depths");
		}

		if (_topShelf.Width != _bottomShelf.Width
			|| _horizontalPanels.Any(p => p.Width != _topShelf.Width)) {
			throw new InvalidOperationException("Horizontal panels in cubby do not have matching widths");
		}

		if (_topShelf.Color != _bottomShelf.Color
			|| _verticalPanels.Any(p => p.Color != _topShelf.Color)
			|| _horizontalPanels.Any(p => p.Color != _topShelf.Color)) {
			throw new InvalidOperationException("Cubby materials do not match");
		}

		if (_topShelf.WallNum != _bottomShelf.WallNum
			|| _verticalPanels.Any(p => p.WallNum != _topShelf.WallNum)
			|| _horizontalPanels.Any(p => p.WallNum != _topShelf.WallNum)) {
			throw new InvalidOperationException("Cubby parts are not part of the same wall");
		}

		if (_topShelf.SectionNum != _bottomShelf.SectionNum
			|| _verticalPanels.Any(p => p.SectionNum != _topShelf.SectionNum)
			|| _horizontalPanels.Any(p => p.SectionNum != _topShelf.SectionNum)) {
			throw new InvalidOperationException("Cubby parts are not part of the same section");
		}

		int dividerCount = _verticalPanels.Count;

		var material = new ClosetMaterial(_topShelf.Color, ClosetMaterialCore.ParticleBoard);
		string roomName = ClosetProPartMapper.GetRoomName(_topShelf, strategy);
		string edgeBandingColor = _topShelf.InfoRecords
											.Where(i => i.PartName == "Edge Banding")
											.Select(i => i.Color)
											.FirstOrDefault() ?? _topShelf.Color;


		if (!ClosetProPartMapper.TryParseMoneyString(_topShelf.PartCost, out decimal topShelfPrice)) {
			topShelfPrice = 0M;
		}

		DividerShelf topShelf = new() {
			Qty = _topShelf.Quantity,
			UnitPrice = topShelfPrice,
			Color = _topShelf.Color,
			Room = roomName,
			PartNumber = _topShelf.PartNum,
			EdgeBandingColor = edgeBandingColor,
			DividerCount = dividerCount,
			Width = Dimension.FromInches(_topShelf.Width),
			Depth = Dimension.FromInches(_topShelf.Depth),
			Type = DividerShelfType.Top,
		};

		if (!ClosetProPartMapper.TryParseMoneyString(_bottomShelf.PartCost, out decimal bottomShelfPrice)) {
			bottomShelfPrice = 0M;
		}

		DividerShelf bottomShelf = new() {
			Qty = _bottomShelf.Quantity,
			UnitPrice = bottomShelfPrice,
			Color = _bottomShelf.Color,
			Room = roomName,
			PartNumber = _bottomShelf.PartNum,
			EdgeBandingColor = edgeBandingColor,
			DividerCount = dividerCount,
			Width = Dimension.FromInches(_bottomShelf.Width),
			Depth = Dimension.FromInches(_bottomShelf.Depth),
			Type = DividerShelfType.Bottom,
		};


		var dividerPanels = _verticalPanels.Select(p => {

			if (!ClosetProPartMapper.TryParseMoneyString(p.PartCost, out decimal unitPrice)) {
				unitPrice = 0M;
			}

			return new DividerVerticalPanel() {
				Qty = p.Quantity,
				UnitPrice = unitPrice,
				Color = p.Color,
				Room = roomName,
				PartNumber = p.PartNum,
				EdgeBandingColor = edgeBandingColor,
				Height = Dimension.FromInches(p.Height),
				Depth = Dimension.FromInches(p.Depth),
				Drilling = VerticalPanelDrilling.DrilledThrough
			};

		}).ToArray();


		Dimension shelfWidth;
		if (_verticalPanels.Count != 0) {
			shelfWidth = (Dimension.FromInches(_topShelf.Width) - (Dimension.FromInches(0.75) * _verticalPanels.Count)) / (_verticalPanels.Count + 1);
		} else {
			shelfWidth = Dimension.FromInches(_topShelf.Width);
		}

		var fixedShelves = _horizontalPanels.Select(p => {

			if (!ClosetProPartMapper.TryParseMoneyString(p.PartCost, out decimal unitPrice)) {
				unitPrice = 0M;
			}

			int qty = _verticalPanels.Count + 1;
			decimal adjUnitPrice = unitPrice / qty;

			return new Shelf() {
				Qty = qty,
				UnitPrice = adjUnitPrice,
				Color = p.Color,
				Room = roomName,
				PartNumber = p.PartNum,
				EdgeBandingColor = edgeBandingColor,
				Width = shelfWidth,
				Depth = Dimension.FromInches(p.Depth),
				Type = ShelfType.Fixed,
				ExtendBack = false
			};

		}).ToArray();

		return new Cubby() {
			TopDividerShelf = topShelf,
			BottomDividerShelf = bottomShelf,
			DividerPanels = dividerPanels,
			FixedShelves = fixedShelves,
			Material = material,
			EdgeBandingColor = edgeBandingColor,
			Room = roomName,
		};

	}

}