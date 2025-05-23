using ApplicationCore.Features.Orders.ProductDrawings.Models;
using Domain.Infrastructure.Bus;
using Domain.Orders.Persistance;

namespace ApplicationCore.Features.Orders.ProductDrawings.Queries;

public class GetProductDrawings {

    public record Query(Guid ProductId) : IQuery<IEnumerable<ProductDrawingSummary>>;

    public class Handler : QueryHandler<Query, IEnumerable<ProductDrawingSummary>> {

        private readonly IOrderingDbConnectionFactory _factory;

        public Handler(IOrderingDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<IEnumerable<ProductDrawingSummary>>> Handle(Query query) {

            try {

                using var connection = await _factory.CreateConnection();

                var drawings = connection.Query<ProductDrawingSummary>(
                    """
                    SELECT
                        id AS ID,
                        product_id AS ProductId,
                        name AS Name
                    FROM product_drawings
                    WHERE product_id = @ProductId;
                    """, query);

                return Response<IEnumerable<ProductDrawingSummary>>.Success(drawings);

            } catch (Exception ex) {

                return new Error() {
                    Title = "Error Occurred While Getting Product Drawings",
                    Details = ex.Message
                };

            }

        }

    }

}
