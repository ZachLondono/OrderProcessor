using Domain.Orders.Builders;
using Domain.Orders.Entities;
using Domain.Orders.Entities.Products;
using Domain.Orders.Enums;
using Domain.ValueObjects;
using OneOf;
using System.Xml.Serialization;

namespace OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

public class CounterTopModel : ProductOrItemModel {

	[XmlAttribute("groupNumber")]
	public int GroupNumber { get; set; }

	[XmlAttribute("lineNumber")]
	public int LineNumber { get; set; }

	[XmlElement("qty")]
	public int Qty { get; set; }

	[XmlElement("unitPrice")]
	public string UnitPrice { get; set; } = string.Empty;

	[XmlElement("finish")]
	public string Finish { get; set; } = string.Empty;

	[XmlElement("width")]
	public double Width { get; set; } 

	[XmlElement("length")]
	public double Length { get; set; } 

	[XmlElement("edgeBanding")]
	public string EdgeBanding { get; set; } = string.Empty;

	[XmlElement("room")]
	public string Room { get; set; } = string.Empty;

	[XmlArray("productionNotes")]
	[XmlArrayItem(ElementName = "note", Type = typeof(string))]
	public List<string> ProductionNotes { get; set; } = [];

	public int GetProductNumber() => int.Parse($"{GroupNumber}{LineNumber:00}");

    public override OneOf<IProduct, AdditionalItem> CreateProductOrItem(ProductBuilderFactory builderFactory) {

		var unitPrice = AllmoxyXMLOrderProviderHelpers.StringToMoney(UnitPrice);
		var width = Dimension.FromMillimeters(Width);
		var length = Dimension.FromMillimeters(Length);
		var edgeBanding = EdgeBanding switch {
			AllmoxyXMLOrderProviderHelpers.NO_EDGEBANDING => EdgeBandingSides.None,
			AllmoxyXMLOrderProviderHelpers.ALL_EDGEBANDING => EdgeBandingSides.All,
			AllmoxyXMLOrderProviderHelpers.ONE_LONG => EdgeBandingSides.OneLong,
			AllmoxyXMLOrderProviderHelpers.ONE_LONG_ONE_SHORT => EdgeBandingSides.OneLongOneShort,
			AllmoxyXMLOrderProviderHelpers.ONE_LONG_TWO_SHORT => EdgeBandingSides.OneLongTwoShort,
			AllmoxyXMLOrderProviderHelpers.TWO_LONG => EdgeBandingSides.TwoLong,
			AllmoxyXMLOrderProviderHelpers.TWO_LONG_ONE_SHORT => EdgeBandingSides.TwoLongOneShort,
			AllmoxyXMLOrderProviderHelpers.TWO_LONG_TWO_SHORT => EdgeBandingSides.TwoLongTwoShort,
			AllmoxyXMLOrderProviderHelpers.ONE_SHORT => EdgeBandingSides.OneShort,
			AllmoxyXMLOrderProviderHelpers.TWO_SHORT => EdgeBandingSides.TwoShort,
			_ => EdgeBandingSides.All,
		};

		return new CounterTop(Guid.NewGuid(),
								Qty,
								unitPrice,
								GetProductNumber(),
								Room,
								ProductionNotes,
								Finish,
								width,
								length,
								edgeBanding);
    }

}
