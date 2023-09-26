using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared.Data.Ordering;
using Dapper;

namespace ApplicationCore.Features.Orders.ProductDrawings;

public class GetProductDrawings {

    public record Query(Guid ProductId) : IQuery<IEnumerable<ProductDrawing>>;

    public class Handler : QueryHandler<Query, IEnumerable<ProductDrawing>> {

        private readonly IOrderingDbConnectionFactory _factory;

        public Handler(IOrderingDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<IEnumerable<ProductDrawing>>> Handle(Query query) {

            try {

                using var connection = await _factory.CreateConnection();

                var drawings = await connection.QueryAsync<ProductDrawing>(
                    """
                    SELECT
                        id AS ID,
                        product_id AS ProductId,
                        name AS Name,
                        dxf_data AS DXFData
                    FROM product_drawings
                    WHERE product_id = @ProductId;
                    """, query);

                return Response<IEnumerable<ProductDrawing>>.Success(drawings);

            } catch (Exception ex) {

                return new Error() {
                    Title = "Error Occurred While Getting Product Drawings",
                    Details = ex.Message
                };

            }

        }

    }

}
