using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared.Data.Ordering;
using Dapper;

namespace ApplicationCore.Features.Orders.OrderRelationships;

internal class GetRelatedOrders {

    public record Query(Guid OrderId) : IQuery<IEnumerable<RelatedOrder>>;

    public class Handler : QueryHandler<Query, IEnumerable<RelatedOrder>> {

        private readonly IOrderingDbConnectionFactory _factory;

        public Handler(IOrderingDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<IEnumerable<RelatedOrder>>> Handle(Query query) {

            using var connection = await _factory.CreateConnection();

            var orders = await connection.QueryAsync<RelatedOrder>(
                """
                SELECT
                	IIF(order_1_id = @OrderId, order_2_id, order_1_id) AS Id,
                    orders.Number,
                    orders.Name
                FROM order_relationships
                    JOIN orders ON IIF(order_1_id = @OrderId, order_2_id, order_1_id) = orders.id
                WHERE
                	(order_1_id = @OrderId) OR (order_2_id = @OrderId);
                """, query);

            return Response<IEnumerable<RelatedOrder>>.Success(orders);

        }

    }
}
