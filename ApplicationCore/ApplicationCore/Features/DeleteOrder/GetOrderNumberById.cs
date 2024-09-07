using Dapper;
using Domain.Infrastructure.Bus;
using Domain.Orders.Persistance;

namespace ApplicationCore.Features.DeleteOrder;

internal class GetOrderNumberById {

    public record Query(Guid Id) : IQuery<string>;

    public class Handler : QueryHandler<Query, string> {

        private readonly IOrderingDbConnectionFactory _factory;

        public Handler(IOrderingDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<string>> Handle(Query query) {

            using var connection = await _factory.CreateConnection();

            string? name = connection.QuerySingleOrDefault<string>("SELECT number FROM orders WHERE id = @Id", query);

            if (name is null) {

                return new Error() {
                    Title = "Not Found",
                    Details = "No order was found with the given id"
                };

            }

            return name;

        }

    }

}
