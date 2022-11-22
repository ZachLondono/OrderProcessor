using ApplicationCore.Features.Orders.Domain;
using ApplicationCore.Features.Orders.Queries.DataModels;
using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Features.Orders.Queries;

public class GetOrderById {

    public record Query(Guid OrderId) : IQuery<Order>;

    public class Handler : QueryHandler<Query, Order> {

        private readonly IDbConnectionFactory _factory;

        public Handler (IDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<Order>> Handle(Query request) {

            using var connection = _factory.CreateConnection();

            
            const string boxquery = @"SELECT
                                        id, lineinorder, unitprice, qty, height_mm as height, width_mm as width, depth_mm as depth, note, labelfields, postfinish, scoopfront, logo, facemountingholes, uboxdimensions, fixeddividers, clips, notches, accessory,
                                        boxmaterialid, (SELECT name FROM drawerboxmaterials WHERE id = boxmaterialid) AS boxmaterialname, (SELECT thickness_mm FROM drawerboxmaterials WHERE id = boxmaterialid) AS boxmaterialthickness ,
                                        bottommaterialid, (SELECT name FROM drawerboxmaterials WHERE id = bottommaterialid) AS bottommaterialname, (SELECT thickness_mm FROM drawerboxmaterials WHERE id = bottommaterialid) AS bottommaterialthickness
                                    FROM drawerboxes
                                    WHERE orderid = @OrderId;";
            var boxData = await connection.QueryAsync<DrawerBoxDataModel>(boxquery, request);
            var boxes = boxData.Select(b => b.AsDomainModel()).ToList();

            const string itemQuery = "SELECT id, description, price FROM additionalitems WHERE orderid = @OrderId;";
            var itemData = await connection.QueryAsync<AdditionalItemDataModel>(itemQuery, request);
            var items = itemData.Select(i => i.AsDomainModel());

            const string query = "SELECT status, number, name, customerid, vendorid, productionNote, customercomment, orderdate, releasedate, productiondate, completedate, info, tax, shipping, priceadjustment, rush FROM orders WHERE id = @OrderId;";
            var data = await connection.QuerySingleAsync<OrderDataModel>(query, request);
            var order = data.AsDomainModel(request.OrderId, boxes, items);

            return new(order);

        }

    }

}
