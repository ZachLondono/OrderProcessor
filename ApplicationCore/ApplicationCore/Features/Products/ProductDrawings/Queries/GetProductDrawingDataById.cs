using Domain.Infrastructure.Bus;
using Domain.Orders.Persistance;

namespace ApplicationCore.Features.Orders.ProductDrawings.Queries;

public class GetProductDrawingDataById {

    public record Query(Guid DrawingId) : IQuery<byte[]>;

    public class Handler : QueryHandler<Query, byte[]> {

        private readonly IOrderingDbConnectionFactory _factory;

        public Handler(IOrderingDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<byte[]>> Handle(Query query) {

            try {

                using var connection = await _factory.CreateConnection();

                var data = connection.QuerySingle<byte[]>(
                    """
                    SELECT
                        dxf_data
                    FROM product_drawings
                    WHERE id = @DrawingId;
                    """, query);

                return Response<byte[]>.Success(data);

            } catch (Exception ex) {

                return new Error() {
                    Title = "Error Occurred While Getting Product Drawings",
                    Details = ex.Message
                };

            }

        }

    }

}
