using Dapper;
using Domain.Infrastructure.Bus;
using Domain.Orders.Persistance;

namespace ApplicationCore.Features.Orders.ProductDrawings.Queries;

public class GetProductDrawingsCount {

    public record Query(Guid ProductId) : IQuery<int>;

    public class Handler : QueryHandler<Query, int> {

        private readonly IOrderingDbConnectionFactory _factory;

        public Handler(IOrderingDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<int>> Handle(Query query) {

            try {

                using var connection = await _factory.CreateConnection();

                int count = await connection.QuerySingleOrDefaultAsync<int>(
                    """
                    SELECT
                        COUNT(*)
                    FROM product_drawings
                    WHERE product_id = @ProductId;
                    """, query);

                return count;

            } catch (Exception ex) {

                return new Error() {
                    Title = "Error Occurred While Getting Product Drawing Count",
                    Details = ex.Message
                };

            }

        }

    }

}
