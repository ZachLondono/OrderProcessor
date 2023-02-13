using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Features.WorkOrders;

internal class IsProductComplete {

    public record Query(Guid OrderId, Guid ProductId) : IQuery<bool>;

    public class Handler : QueryHandler<Query, bool> {

        public readonly IDbConnectionFactory _factory;

        public Handler(IDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<bool>> Handle(Query query) {

            using var connection = _factory.CreateConnection();

            const string sql = @"SELECT
	                                (SELECT COUNT(*)
		                                FROM work_order_products
		                                LEFT JOIN work_orders ON work_orders.id = work_order_products.work_order_id
		                                WHERE work_orders.order_id = @OrderId AND product_id = @ProductId AND work_orders.status != @CompleteStatus) As InCompleteCount,
	                                (SELECT COUNT(*)
		                                FROM work_order_products
		                                WHERE product_id = @ProductId) As TotalCount";

            var result = await connection.QuerySingleAsync<(int InComplete, int Total)>(sql, new {
                query.OrderId,
                query.ProductId,
                CompleteStatus = Status.Complete
            });

            var isComplete = (result.Total > 0) && (result.InComplete == 0);

            return Response<bool>.Success(isComplete);

        }

    }

}