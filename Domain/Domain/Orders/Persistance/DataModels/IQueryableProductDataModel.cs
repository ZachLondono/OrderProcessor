namespace Domain.Orders.Persistance.DataModels;

public interface IQueryableProductDataModel : IProductDataModel {

    public static abstract string GetQueryByOrderId { get; }

}