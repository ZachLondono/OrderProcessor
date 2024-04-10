using Domain.Orders.Builders;
using Domain.Orders.Entities;
using Domain.Orders.Enums;
using Domain.Orders.Entities.Products.Closets;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;
using OneOf;
using System.Xml.Serialization;
using ClosetPaintedSide = Domain.Orders.ValueObjects.PaintedSide;

namespace OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

public class CustomDrillingVerticalPanelModel : ProductOrItemModel {

	[XmlAttribute("groupNumber")]
	public int GroupNumber { get; set; }

	[XmlAttribute("lineNumber")]
	public int LineNumber { get; set; }

	[XmlElement("room")]
	public string Room { get; set; } = string.Empty;

	[XmlElement("width")]
	public double Width { get; set; }

	[XmlElement("length")]
	public double Length { get; set; }

	[XmlElement("qty")]
	public int Qty { get; set; }

	[XmlElement("unitPrice")]
	public string UnitPrice { get; set; } = string.Empty;

	[XmlElement("edgeBandColor")]
	public string EdgeBandColor { get; set; } = string.Empty;

	[XmlElement("materialFinish")]
	public string MaterialFinish { get; set; } = string.Empty;

	[XmlElement("materialCore")]
	public string MaterialCore { get; set; } = string.Empty;

	[XmlElement("paintColor")]
	public string PaintColor { get; set; } = string.Empty;

	[XmlElement("paintedSide")]
	public string PaintedSide { get; set; } = string.Empty;

	[XmlElement("comment")]
	public string Comment { get; set; } = string.Empty;

	[XmlElement("drillingType")]
	public string DrillingType { get; set; } = string.Empty;

	[XmlElement("extendBack")]
	public double ExtendBack { get; set; }

	[XmlElement("extendFront")]
	public double ExtendFront { get; set; }

	[XmlElement("holeDimensionFromBottom")]
	public double HoleDimensionFromBottom { get; set; }

	[XmlElement("holeDimensionFromTop")]
	public double HoleDimensionFromTop { get; set; }

	[XmlElement("transitionHoleDimensionFromBottom")]
	public double TransitionHoleDimensionFromBottom { get; set; }

	[XmlElement("transitionHoleDimensionFromTop")]
	public double TransitionHoleDimensionFromTop { get; set; }

	[XmlElement("bottomNotchDepth")]
	public double BottomNotchDepth { get; set; }

	[XmlElement("bottomNotchHeight")]
	public double BottomNotchHeight { get; set; }

	[XmlElement("ledChannelFront")]
	public double LEDChannelOffFront { get; set; }

	[XmlElement("ledChannelWidth")]
	public double LEDChannelWidth { get; set; }

	[XmlElement("ledChannelDepth")]
	public double LEDChannelDepth { get; set; }

	[XmlArray("productionNotes")]
	[XmlArrayItem(ElementName = "note", Type = typeof(string))]
	public List<string> ProductionNotes { get; set; } = new();

	public int GetProductNumber() => int.Parse($"{GroupNumber}{LineNumber:00}");

	public override OneOf<IProduct, AdditionalItem> CreateProductOrItem(ProductBuilderFactory builderFactory) {

		ClosetMaterialCore core = MaterialCore switch {
			AllmoxyXMLOrderProviderHelpers.PARTICLE_BOARD_CORE_CODE => ClosetMaterialCore.ParticleBoard,
			AllmoxyXMLOrderProviderHelpers.PLYWOOD_CORE_CODE => ClosetMaterialCore.Plywood,
			_ => throw new InvalidOperationException($"Unexpected material core type '{MaterialCore}'"),
		};

		ClosetMaterial material = new(MaterialFinish, core);

		string? paintColor = string.IsNullOrWhiteSpace(PaintColor) ? null : PaintColor;
		ClosetPaint? paint = null;
		if (paintColor is not null) {
			if (Enum.TryParse(PaintedSide, out ClosetPaintedSide paintedSide)) {
				paint = new(paintColor, paintedSide);
			} else {
				paint = new(paintColor, ClosetPaintedSide.Custom);
			}

		}

		Dimension width = Dimension.FromMillimeters(Width);
		Dimension length = Dimension.FromMillimeters(Length);

		decimal unitPrice = AllmoxyXMLOrderProviderHelpers.StringToMoney(UnitPrice);

		string edgeBandColor = EdgeBandColor == "Match" ? MaterialFinish : EdgeBandColor;

		ClosetVerticalDrillingType drillingType = DrillingType switch {
			"finished left" => ClosetVerticalDrillingType.FinishedLeft,
			"finished right" => ClosetVerticalDrillingType.FinishedRight,
			"drilled through" or _ => ClosetVerticalDrillingType.DrilledThrough,
		};

		Dimension extendBack = Dimension.FromMillimeters(ExtendBack);
		Dimension extendFront = Dimension.FromMillimeters(ExtendFront);
		Dimension holeDimensionFromBottom = Dimension.FromMillimeters(HoleDimensionFromBottom);
		Dimension holeDimensionFromTop = Dimension.FromMillimeters(HoleDimensionFromTop);
		Dimension transitionHoleDimensionFromBottom = Dimension.FromMillimeters(TransitionHoleDimensionFromBottom);
		Dimension transitionHoleDimensionFromTop = Dimension.FromMillimeters(TransitionHoleDimensionFromTop);
		Dimension bottomNotchHeight = Dimension.FromMillimeters(BottomNotchHeight);
		Dimension bottomNotchDepth = Dimension.FromMillimeters(BottomNotchDepth);
		Dimension ledChannelOffFront = Dimension.FromMillimeters(LEDChannelOffFront);
		Dimension ledChannelWidth = Dimension.FromMillimeters(LEDChannelWidth);
		Dimension ledChannelDepth = Dimension.FromMillimeters(LEDChannelDepth);

		if (holeDimensionFromTop + holeDimensionFromBottom > length) {
			holeDimensionFromTop = Dimension.Zero;
			holeDimensionFromBottom = Dimension.Zero;
		}

		return new CustomDrilledVerticalPanel(Guid.NewGuid(), Qty, unitPrice, GetProductNumber(), Room, width, length, material, paint, edgeBandColor, Comment, drillingType, extendBack, extendFront, holeDimensionFromBottom, holeDimensionFromTop, transitionHoleDimensionFromBottom, transitionHoleDimensionFromTop, bottomNotchDepth, bottomNotchHeight, ledChannelOffFront, ledChannelWidth, ledChannelDepth) {
			ProductionNotes = ProductionNotes.Where(n => !string.IsNullOrWhiteSpace(n)).ToList()
		};

	}

}
