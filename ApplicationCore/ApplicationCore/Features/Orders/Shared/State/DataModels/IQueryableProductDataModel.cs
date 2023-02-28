namespace ApplicationCore.Features.Orders.Shared.State.DataModels;

internal interface IQueryableProductDataModel : IProductDataModel {

    public static abstract string GetQueryByOrderId { get; }

}