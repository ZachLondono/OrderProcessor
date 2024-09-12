using Domain.Orders.Entities.Products;
using Domain.ValueObjects;
using Domain.Orders.Enums;
using Domain.Orders.Entities;

namespace Domain.Orders.Persistance.DataModels;

public class CounterTopDataModel : ProductDataModelBase, IProductDataModel, IQueryableProductDataModel {

    public string Finish { get; set; } = string.Empty;
    public Dimension Width { get; set; }
    public Dimension Length { get; set; }
    public EdgeBandingSides EdgeBanding { get; set; }

    public static string GetQueryByOrderId =>
        """
        SELECT

            products.id,
            products.qty,
            products.unit_price AS UnitPrice,
            products.product_number AS ProductNumber,
            products.room,

            counter_tops.finish,
            counter_tops.width,
            counter_tops.length,
            counter_tops.edge_banding AS EdgeBanding

        FROM counter_tops

            JOIN products ON counter_tops.product_id = products.id

        WHERE products.order_id = @OrderId;

        """;

    public IProduct MapToProduct(IEnumerable<ProductionNote> productionNotes) {

        return new CounterTop(Id,
                              Qty,
                              UnitPrice,
                              ProductNumber,
                              Room,
                              productionNotes.ToList(),
                              Finish,
                              Width,
                              Length,
                              EdgeBanding);

    }

}
