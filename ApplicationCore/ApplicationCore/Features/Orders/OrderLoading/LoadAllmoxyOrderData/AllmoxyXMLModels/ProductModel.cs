using ApplicationCore.Features.Orders.Shared.Domain.Builders;
using ApplicationCore.Features.Orders.Shared.Domain.Products;

namespace ApplicationCore.Features.Orders.OrderLoading.LoadAllmoxyOrderData.AllmoxyXMLModels;

// Have to use an abstract class instead of an interface because XMLSerializer cannot deserialize an array of items into a List of interfaces
public abstract class ProductModel {

    public abstract IProduct CreateProduct(ProductBuilderFactory builderFactory);

}