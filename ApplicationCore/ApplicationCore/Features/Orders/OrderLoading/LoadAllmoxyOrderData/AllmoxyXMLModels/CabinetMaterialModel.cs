using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

public class CabinetMaterialModel {

    /// <summary>
    /// The type of wood used as the substrate for the material. For example 'pb' for particle board and 'ply' for plywood.
    /// </summary>
    [XmlElement("core")]
    public string Core { get; set; } = string.Empty;

    /// <summary>
    /// Describes the type of finish which is applied to the substrate. For example, 'mela' for melamine paper, 'veneer' for wood veneer or 'laminate' for laminate.
    /// </summary>
    [XmlElement("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// The name or color of the material's finish. This may be a generic color like 'White', a vendor-specific finish like 'Tafisa Urbania White Chocolate' or a paint color
    /// </summary>
    [XmlElement("finish")]
    public string Finish { get; set; } = string.Empty;
}
