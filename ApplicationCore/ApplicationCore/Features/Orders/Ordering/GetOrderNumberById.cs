using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Features.Orders.Ordering;

internal class GetOrderNumberById {

    public record Query(Guid Id) : IQuery<string>;

    public class Handler : QueryHandler<Query, string> {

        private readonly IDbConnectionFactory _factory;

        public Handler(IDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<string>> Handle(Query query) {
            
            using var connection = _factory.CreateConnection();

            string? name = await connection.QuerySingleOrDefaultAsync<string>("SELECT number FROM orders WHERE id = @Id", query);

            if (name is null) {

                return Response<string>.Error(new() {
                    Title = "Not Found",
                    Details = "No order was found with the given id"
                });

            }

            return Response<string>.Success(name);

        }

    }

}
