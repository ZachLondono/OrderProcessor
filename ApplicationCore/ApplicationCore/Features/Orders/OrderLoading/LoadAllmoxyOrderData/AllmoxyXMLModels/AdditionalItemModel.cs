using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using OneOf;
using System.Xml.Serialization;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

public class AdditionalItemModel : ProductOrItemModel {

    [XmlElement("qty")]
    public int Qty { get; set; }

    [XmlElement("description")]
    public string Description { get; set; } = string.Empty;

    [XmlElement("price")]
    public string UnitPrice { get; set; } = string.Empty;

    public override OneOf<IProduct, AdditionalItem> CreateProductOrItem(ProductBuilderFactory builderFactory) {
        var unitPrice = AllmoxyXMLOrderProviderHelpers.StringToMoney(UnitPrice);
        return AdditionalItem.Create(Qty, Description, unitPrice);
    }

}
