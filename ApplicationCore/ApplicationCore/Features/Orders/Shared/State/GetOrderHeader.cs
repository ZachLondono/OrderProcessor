using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared.Data.Ordering;
using Dapper;

namespace ApplicationCore.Features.Orders.Shared.State;

public class GetOrderHeader {

    public record Query(Guid OrderId) : IQuery<OrderHeader>;

    public class OrderHeader {
        public Guid OrderId { get; init; }
        public string Number { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string CustomerComment { get; init; } = string.Empty;
        public Guid VendorId  { get; init; }
        public Guid CustomerId  { get; init; }
        public DateTime OrderDate  { get; init; }
        public DateTime? DueDate { get; init; } = null;
    }

    public class Handler : QueryHandler<Query, OrderHeader> {

        private readonly IOrderingDbConnectionFactory _factory;

        public Handler(IOrderingDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<OrderHeader>> Handle(Query query) {

            using var connection = await _factory.CreateConnection();

            return await connection.QuerySingleAsync<OrderHeader>(
                """
                SELECT 
                    id,
                    number,
                    name,
                    customer_comment AS CustomerComment,
                    vendor_id AS VendorId,
                    customer_id AS CustomerId,
                    order_date AS OrderDate,
                    due_date AS DueDate
                FROM orders
                WHERE id = @OrderId;
                """,
                query);

        }

    }

}
