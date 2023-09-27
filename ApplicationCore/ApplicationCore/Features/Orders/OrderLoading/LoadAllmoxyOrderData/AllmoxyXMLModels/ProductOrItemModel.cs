using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Entities;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using OneOf;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

// Have to use an abstract class instead of an interface because XMLSerializer cannot deserialize an array of items into a List of interfaces
public abstract class ProductOrItemModel {

    public abstract OneOf<IProduct, AdditionalItem> CreateProductOrItem(ProductBuilderFactory builderFactory);

}