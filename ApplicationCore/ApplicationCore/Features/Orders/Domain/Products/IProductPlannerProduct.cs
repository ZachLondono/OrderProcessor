namespace ApplicationCore.Features.Orders.Domain.Products;

public interface IProductPlannerProduct {

    string Room { get; }

    string GetProductName();

    Dictionary<string, string> GetParameters();

    Dictionary<string, string> GetOverrideParameters();

}