using ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData;
using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Doors;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;
using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

public class FivePieceDoorModel : ProductModel {

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

    [XmlElement("comment")]
    public string Comment { get; set; } = string.Empty;

    [XmlElement("materialFinish")]
    public string MaterialFinish { get; set; } = string.Empty;

    [XmlElement("materialCore")]
    public string MaterialCore { get; set; } = string.Empty;

    [XmlElement("rails")]
    public double Rails { get; set; }

    [XmlElement("height")]
    public double Height { get; set; }

    [XmlElement("width")]
    public double Width { get; set; }
    
    public int GetProductNumber() => int.Parse($"{GroupNumber}{LineNumber:00}");

    public override IProduct CreateProduct(ProductBuilderFactory builderFactory) {

        Dimension width = Dimension.FromMillimeters(Width);
        Dimension height = Dimension.FromMillimeters(Height);
        string material = $"{MaterialFinish} - {MaterialCore}";
        var frameSize = new DoorFrame(Dimension.FromMillimeters(Rails));
        var frameThickness = Dimension.FromMillimeters(19.05);
        var panelThickness = Dimension.FromMillimeters(6.35);

        decimal unitPrice = AllmoxyXMLOrderProviderHelpers.StringToMoney(UnitPrice);

        return new FivePieceDoorProduct(Guid.NewGuid(), Qty, unitPrice, GetProductNumber(), Room, width, height, frameSize, frameThickness, panelThickness, material);

    }

}
