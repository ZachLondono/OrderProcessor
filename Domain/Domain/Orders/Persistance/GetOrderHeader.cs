using Dapper;
using Domain.Infrastructure.Bus;

namespace Domain.Orders.Persistance;

public class GetOrderHeader {

    public record Query(Guid OrderId) : IQuery<OrderHeader>;

    public class OrderHeader {
        public Guid OrderId { get; init; }
        public string Number { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string CustomerComment { get; init; } = string.Empty;
        public Guid VendorId { get; init; }
        public Guid CustomerId { get; init; }
        public DateTime OrderDate { get; init; }
        public DateTime? DueDate { get; init; } = null;
        public bool Rush { get; init; } = false;
    }

    public class Handler : QueryHandler<Query, OrderHeader> {

        private readonly IOrderingDbConnectionFactory _factory;

        public Handler(IOrderingDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<OrderHeader>> Handle(Query query) {

            using var connection = await _factory.CreateConnection();

            var header = connection.QuerySingle<OrderHeader>(
                """
                SELECT 
                    id AS OrderId,
                    number,
                    name,
                    customer_comment AS CustomerComment,
                    vendor_id AS VendorId,
                    customer_id AS CustomerId,
                    order_date AS OrderDate,
                    due_date AS DueDate,
                    rush
                FROM orders
                WHERE id = @OrderId;
                """,
                query);

            return header;

        }

    }

}
