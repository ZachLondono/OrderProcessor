using Domain.Orders.Builders;
using Domain.Orders.Entities;
using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.Orders.Entities.Products.DrawerBoxes;
using Domain.ValueObjects;
using OneOf;
using System.Xml.Serialization;

namespace OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

public class DovetailDrawerBoxModel : ProductOrItemModel {

	[XmlAttribute("groupNumber")]
	public int GroupNumber { get; set; }

	[XmlAttribute("lineNumber")]
	public int LineNumber { get; set; }

	[XmlElement("comments")]
	public string Comments { get; set; } = string.Empty;

	[XmlElement("material")]
	public string Material { get; set; } = string.Empty;

	[XmlElement("bottom")]
	public string Bottom { get; set; } = string.Empty;

	[XmlElement("clips")]
	public string Clips { get; set; } = string.Empty;

	[XmlElement("notch")]
	public string Notch { get; set; } = string.Empty;

	[XmlElement("insert")]
	public string Insert { get; set; } = string.Empty;

	[XmlElement("unitPrice")]
	public string UnitPrice { get; set; } = string.Empty;

	[XmlElement("qty")]
	public int Qty { get; set; }

	[XmlElement("logo")]
	public string Logo { get; set; } = string.Empty;

	[XmlElement("scoop")]
	public string Scoop { get; set; } = string.Empty;

	[XmlElement("note")]
	public string Note { get; set; } = string.Empty;

	[XmlElement("height")]
	public double Height { get; set; }

	[XmlElement("width")]
	public double Width { get; set; }

	[XmlElement("depth")]
	public double Depth { get; set; }

	[XmlElement("room")]
	public string Room { get; set; } = string.Empty;

	[XmlArray("productionNotes")]
	[XmlArrayItem(ElementName = "note", Type = typeof(string))]
	public List<string> ProductionNotes { get; set; } = new();

	public int GetProductNumber() => int.Parse($"{GroupNumber}{LineNumber:00}");

	private readonly Dictionary<string, string> _notchTypes = new() {
	   { "No Notch",  "No_Notch" },
	   { "Notch for Standard U/M Slide",  "Std_Notch" },
	   { "Notch for U/M Slide, Wide",  "Wide_Notch" },
	   { "Notch for 828",  "Notch_828" },
       //{ "",  "Unknown" },
       //{ "",  "Wide_Notch_F70" },
       //{ "",  "Front_Back" },
    };

	public override OneOf<IProduct, AdditionalItem> CreateProductOrItem(ProductBuilderFactory builderFactory) {

		decimal unitPrice = AllmoxyXMLOrderProviderHelpers.StringToMoney(UnitPrice);

		var height = Dimension.FromMillimeters(Height);
		var width = Dimension.FromMillimeters(Width);
		var depth = Dimension.FromMillimeters(Depth);

		var labelFields = new Dictionary<string, string>();

		if (Material == "Pre-Finished Birch")
			Material = DovetailDrawerBoxConfig.SOLID_BIRCH;
		else if (Material == "Economy Birch")
			Material = DovetailDrawerBoxConfig.FINGER_JOINT_BIRCH;

		var productionNotes = ProductionNotes.Where(n => !string.IsNullOrWhiteSpace(n)).Select(p => ProductionNote.Create(p)).ToList();

		if (!_notchTypes.TryGetValue(Notch, out string? notch)) {
			notch = Notch;
			productionNotes.Add(ProductionNote.Create("CHECK NOTCH TYPE"));
		}

		LogoPosition logo = Logo switch {
			"Yes" => LogoPosition.Inside,
			_ => LogoPosition.None
		};

		if (Logo != "Yes" && Logo != "No") {
			productionNotes.Add(ProductionNote.Create("CHECK LOGO OPTION"));
		}

		var options = new DovetailDrawerBoxConfig(Material, Material, Material, Bottom, Clips, notch, Insert, logo);

		var product = DovetailDrawerBoxProduct.Create(unitPrice, Qty, Room, GetProductNumber(), height, width, depth, Note, labelFields, options);
		product.ProductionNotes = productionNotes;

		return product;

	}

}
