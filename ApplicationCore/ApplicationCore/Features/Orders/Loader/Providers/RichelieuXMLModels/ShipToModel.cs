using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.RichelieuXMLModels;

public class ShipToModel {

    [XmlAttribute("richelieuNumber")]
    public required string RichelieuNumber {get; set;}

    [XmlAttribute("firstName")]
    public required string FirstName {get; set;}

    [XmlAttribute("lastName")]
    public required string LastName {get; set;}

    [XmlAttribute("company")]
    public required string Company {get; set;}

    [XmlAttribute("address1")]
    public required string Address1 {get; set;}

    [XmlAttribute("address2")]
    public required string Address2 {get; set;}

    [XmlAttribute("city")]
    public required string City {get; set;}

    [XmlAttribute("province")]
    public required string Province {get; set;}

    [XmlAttribute("postalCode")]
    public required string PostalCode {get; set;}

    [XmlAttribute("country")]
    public required string Country {get; set;}

    [XmlAttribute("phone")]
    public required string Phone {get; set;}

    [XmlAttribute("fax")]
    public required string Fax {get; set;}

    [XmlAttribute("mobile")]
    public required string Mobile {get; set;}

    [XmlAttribute("irsNumber")]
    public required string IRSNumber { get; set; }


}
