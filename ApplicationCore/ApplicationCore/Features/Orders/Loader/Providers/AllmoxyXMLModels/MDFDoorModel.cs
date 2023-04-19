using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;
using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;

public class MDFDoorModel : ProductModel {

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

    [XmlElement("bottomRail")]
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

    public int GetProductNumber() => int.Parse($"{GroupNumber}{LineNumber:00}");

    public override IProduct CreateProduct(ProductBuilderFactory builderFactory) {

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
            additionalOpenings.Add(new(Dimension.FromMillimeters(Rail3), Dimension.FromMillimeters(Opening1)));
        }

        if (Opening2 > 0) {
            additionalOpenings.Add(new(Dimension.FromMillimeters(Rail4), Dimension.FromMillimeters(Opening2)));
        }

        return MDFDoorProduct.Create(unitPrice, Room, Qty, GetProductNumber(), type, height, width, Note, frameSize, Material, thickness, FramingBead, EdgeProfile, PanelDetail, panelDrop, orientation, additionalOpenings.ToArray(), Finish);

    }
}
