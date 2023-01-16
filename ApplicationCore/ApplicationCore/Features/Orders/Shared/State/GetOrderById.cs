using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.State.DataModels;
using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Features.Orders.Shared.State;

public class GetOrderById {

    public record Query(Guid OrderId) : IQuery<Order>;

    public class Handler : QueryHandler<Query, Order> {

        private readonly IDbConnectionFactory _factory;

        public Handler(IDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<Order>> Handle(Query request) {

            using var connection = _factory.CreateConnection();

            const string itemQuery = "SELECT id, description, price FROM additionalitems WHERE orderid = @OrderId;";
            var itemData = await connection.QueryAsync<AdditionalItemDataModel>(itemQuery, request);
            var items = itemData.Select(i => i.AsDomainModel());

            const string query = "SELECT status, number, name, customerid, vendorid, productionNote, customercomment, orderdate, releasedate, productiondate, completedate, info, tax, shipping, priceadjustment, rush FROM orders WHERE id = @OrderId;";
            var data = await connection.QuerySingleAsync<OrderDataModel>(query, request);
            var order = data.AsDomainModel(request.OrderId, new List<IProduct>(), items);

            return new(order);

        }

    }

}
