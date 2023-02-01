using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.Loader.Providers.AllmoxyXMLModels;

public class DrawerBoxModel : ProductModel {

    [XmlElement("qty")]
    public int Qty { get; set; }

    [XmlElement("unitPrice")]
    public decimal UnitPrice { get; set; }

    public override IProduct CreateProduct(ProductBuilderFactory builderFactory) {
        throw new NotImplementedException();
    }

}