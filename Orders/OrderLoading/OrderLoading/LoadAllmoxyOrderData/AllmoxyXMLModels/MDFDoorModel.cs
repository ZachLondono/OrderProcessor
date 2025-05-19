using Domain.Orders.Builders;
using Domain.Orders.Entities;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.Orders.Entities.Products.Doors;
using Domain.ValueObjects;
using OneOf;
using System.Xml.Serialization;
using OneOf.Types;

namespace OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

public class MDFDoorModel : ProductOrItemModel {

	[XmlAttribute("groupNumber")]
	public int GroupNumber { get; set; }

	[XmlAttribute("lineNumber")]
	public int LineNumber { get; set; }

	[XmlElement("qty")]
	public int Qty { get; set; }

	[XmlElement("unitPrice")]
	public string UnitPrice { get; set; } = string.Empty;

	[XmlElement("room")]
	public string Room { get; set; } = string.Empty;

	[XmlElement("width")]
	public double Width { get; set; }

	[XmlElement("height")]
	public double Height { get; set; }

	[XmlElement("topRail")]
	public double TopRail { get; set; }

	[XmlElement("botRail")]
	public double BottomRail { get; set; }

	[XmlElement("leftStile")]
	public double LeftStile { get; set; }

	[XmlElement("rightStile")]
	public double RightStile { get; set; }

	[XmlElement("rail3")]
	public double Rail3 { get; set; }

	[XmlElement("rail4")]
	public double Rail4 { get; set; }

	[XmlElement("opening1")]
	public double Opening1 { get; set; }

	[XmlElement("opening2")]
	public double Opening2 { get; set; }

	[XmlElement("frameOnly")]
	public string FrameOnly { get; set; } = string.Empty;

	[XmlElement("note")]
	public string Note { get; set; } = string.Empty;

	[XmlElement("framingBead")]
	public string FramingBead { get; set; } = string.Empty;

	[XmlElement("edgeProfile")]
	public string EdgeProfile { get; set; } = string.Empty;

	[XmlElement("panelDetail")]
	public string PanelDetail { get; set; } = string.Empty;

	[XmlElement("material")]
	public string Material { get; set; } = string.Empty;

	[XmlElement("finish")]
	public string Finish { get; set; } = string.Empty;

	[XmlElement("customPanelDrop")]
	public string CustomPanelDrop { get; set; } = string.Empty;

	[XmlArray("productionNotes")]
	[XmlArrayItem(ElementName = "note", Type = typeof(string))]
	public List<string> ProductionNotes { get; set; } = new();

	public int GetProductNumber() => int.Parse($"{GroupNumber}{LineNumber:00}");

	public override OneOf<IProduct, AdditionalItem> CreateProductOrItem(ProductBuilderFactory builderFactory) {

		decimal unitPrice = AllmoxyXMLOrderProviderHelpers.StringToMoney(UnitPrice);
		var type = DoorType.Door;
		var height = Dimension.FromMillimeters(Height);
		var width = Dimension.FromMillimeters(Width);
		DoorFrame frameSize = new() {
			TopRail = Dimension.FromMillimeters(TopRail),
			BottomRail = Dimension.FromMillimeters(BottomRail),
			LeftStile = Dimension.FromMillimeters(LeftStile),
			RightStile = Dimension.FromMillimeters(RightStile)
		};
		Dimension panelDrop = Dimension.Zero;
		if (double.TryParse(CustomPanelDrop, out var value)) {
			panelDrop = Dimension.FromMillimeters(value);
		}

		Dimension thickness = Dimension.FromInches(0.75);

		var orientation = DoorOrientation.Vertical;

		var additionalOpenings = new List<AdditionalOpening>();
		if (Opening1 > 0) {
			additionalOpenings.Add(new(Dimension.FromMillimeters(Rail3), Dimension.FromMillimeters(Opening1), new SolidPanel()));
		}

		if (Opening2 > 0) {
			additionalOpenings.Add(new(Dimension.FromMillimeters(Rail4), Dimension.FromMillimeters(Opening2), new SolidPanel()));
		}

		MDFDoorFinish finish = Finish switch {
			"Un-Sanded" => throw new NotImplementedException(),
			"Sanded" => new None(),
			"Primed" => new Primer(),
			"Pure White" => new Paint("Pure White"),
			"Frosty White" => new Paint("Frosty White"),
			"White Mela Match" => new Paint("White Mela Match"),
			"BM Gray 2121" => new Paint("BM Gray 2121"),
            "Custom Color (comment color, +$50 mixing charge)" => new Paint("Custom Color"),
			_ => throw new InvalidOperationException($"Unknown finish type {Finish}")
        };

		var product = MDFDoorProduct.Create(unitPrice, Room, Qty, GetProductNumber(), type, height, width, Note, frameSize, Material, thickness, FramingBead, EdgeProfile, PanelDetail, panelDrop, orientation, additionalOpenings.ToArray(), finish, new SolidPanel());
		product.ProductionNotes = ProductionNotes.Where(n => !string.IsNullOrWhiteSpace(n)).ToList();
		return product;

	}
}
