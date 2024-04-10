using Domain.Orders.Builders;
using Domain.Orders.Entities;
using Domain.Orders.Entities.Products;
using OneOf;

namespace OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

// Have to use an abstract class instead of an interface because XMLSerializer cannot deserialize an array of items into a List of interfaces
public abstract class ProductOrItemModel {

	public abstract OneOf<IProduct, AdditionalItem> CreateProductOrItem(ProductBuilderFactory builderFactory);

}