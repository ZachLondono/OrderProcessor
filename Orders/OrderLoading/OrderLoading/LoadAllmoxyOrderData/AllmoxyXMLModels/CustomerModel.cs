using System.Xml.Serialization;

namespace OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

public class CustomerModel {

    [XmlAttribute("id")]
    public int CompanyId { get; set; }

    [XmlText]
    public string Company { get; set; } = string.Empty;

}
