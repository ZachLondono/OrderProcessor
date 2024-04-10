using Domain.Orders.Builders;
using Domain.Orders.Entities;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;
using OneOf;
using System.Xml.Serialization;
using Domain.Orders.Entities.Products.DrawerBoxes;

namespace OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

public class DoweledDrawerBoxModel : ProductOrItemModel {

	[XmlAttribute("groupNumber")]
	public int GroupNumber { get; set; }

	[XmlAttribute("lineNumber")]
	public int LineNumber { get; set; }

	[XmlElement("qty")]
	public int Qty { get; set; }

	[XmlElement("unitPrice")]
	public decimal UnitPrice { get; set; }

	[XmlElement("height")]
	public double Height { get; set; }

	[XmlElement("width")]
	public double Width { get; set; }

	[XmlElement("depth")]
	public double Depth { get; set; }

	[XmlElement("materialName")]
	public string MaterialName { get; set; } = string.Empty;

	[XmlElement("boxThickness")]
	public double BoxThickness { get; set; }

	[XmlElement("bottomThickness")]
	public double BottomThickness { get; set; }

	[XmlElement("machineForUMSlides")]
	public bool MachineForUMSlides { get; set; }

	[XmlElement("frontBackHeightAdj")]
	public double FrontBackHeightAdj { get; set; }

	[XmlElement("room")]
	public string Room { get; set; } = string.Empty;

	[XmlArray("productNotes")]
	public List<string> ProductionNotes { get; set; } = new();

	public int GetProductNumber() => int.Parse($"{GroupNumber}{LineNumber:00}");

	public override OneOf<IProduct, AdditionalItem> CreateProductOrItem(ProductBuilderFactory builderFactory) {

		var height = Dimension.FromMillimeters(Height);
		var width = Dimension.FromMillimeters(Width);
		var depth = Dimension.FromMillimeters(Depth);

		Dimension boxThickness = Dimension.FromMillimeters(BoxThickness);
		var boxMat = new DoweledDrawerBoxMaterial(MaterialName, boxThickness, true);

		Dimension bottomThickness = Dimension.FromMillimeters(BottomThickness);
		var bottomMat = new DoweledDrawerBoxMaterial(MaterialName, bottomThickness, true);

		var frontBackAdj = Dimension.FromMillimeters(FrontBackHeightAdj);

		return new DoweledDrawerBoxProduct(Guid.NewGuid(), UnitPrice, Qty, Room, GetProductNumber(), height, width, depth, boxMat, boxMat, boxMat, bottomMat, MachineForUMSlides, frontBackAdj) {
			ProductionNotes = ProductionNotes.Where(n => !string.IsNullOrWhiteSpace(n)).ToList(),
		};

	}

}