using Companies.Infrastructure;
using Domain.Infrastructure.Bus;
using Domain.Orders.Persistance;
using RestSharp;

namespace ApplicationCore.Features.ProgressTracker;

public class AddOrderToProgressTracker {

    public record Command(Guid OrderId) : ICommand;

    public class Handler : CommandHandler<Command> {

        private readonly IOrderingDbConnectionFactory _orderDbFactory;
        private readonly ICompaniesDbConnectionFactory _companyDbFactory;

        public Handler(IOrderingDbConnectionFactory orderDbFactory, ICompaniesDbConnectionFactory companyDbFactory) {
            _orderDbFactory = orderDbFactory;
            _companyDbFactory = companyDbFactory;
        }

        public override async Task<Response> Handle(Command command) {

            var client = new RestClient("http://api.zacharylondono.com");

            var trackerClient = new OrderTrackerClient(client);

            var order = await GetOrder(command.OrderId);
            var customer = await GetCustomerName(order.CustomerId);
            var vendor = await GetVendorName(order.VendorId);

            var orderResult = await trackerClient.PostNewOrder(new() {
                Number = order.Number,
                Name = order.Name,
                Customer = customer,
                Vendor = vendor,
                OrderedDate = DateTime.Now,
                WantByDate = DateTime.Now,
                IsRush = false,
                Price = 0,
                Shipping = 0,
                Tax = 0,
                Note = "",
                IsAllmoxyOrder = true
            });

            if (orderResult is null) {
                return new Error() {
                    Title = "Order Was Not Tracked",
                    Details = "Unexpected response returned from server when trying to add new order to progress tracker"
                };
            }

            var releaseResult = await trackerClient.PostNewRelease(orderResult.Id, new() {
                ItemCount = order.ItemCount
            });

            if (releaseResult is null) {
                return new Error() {
                    Title = "Order Was Not Tracked",
                    Details = "Unexpected response returned from server when trying to add new release to progress tracker"
                };
            }

            return Response.Success();

        }

        private async Task<Order> GetOrder(Guid orderId) {

            using var connection = await _orderDbFactory.CreateConnection();

            return connection.QuerySingle<Order>(
                """
                SELECT
                    number,
                    name,
                    order_date AS OrderDate,
                    customer_id AS CustomerId,
                    vendor_id AS VendorId,
                    (SELECT COUNT(*) FROM products WHERE order_id = @OrderId) AS ItemCount 
                FROM
                    orders
                WHERE
                    id = @OrderId;
                """, new {
                    OrderId = orderId
                });


        }

        private async Task<string> GetCustomerName(Guid companyId) {

            using var connection = await _companyDbFactory.CreateConnection();

            return connection.QuerySingle<string>(
                """
                SELECT name FROM customers WHERE id = @CompanyId;
                """, new {
                    CompanyId = companyId
                });

        }

        private async Task<string> GetVendorName(Guid companyId) {

            using var connection = await _companyDbFactory.CreateConnection();

            return connection.QuerySingle<string>(
                """
                SELECT name FROM vendors WHERE id = @CompanyId;
                """, new {
                    CompanyId = companyId
                });

        }

    }

    public class Order {
        public string Number { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public Guid CustomerId { get; set; }
        public Guid VendorId { get; set; }
        public int ItemCount { get; set; }
    }

    public class OrderTrackerClient {

        private readonly RestClient _client;

        public OrderTrackerClient(RestClient client) {
            _client = client;
        }

        public async Task<NewOrderResponse?> PostNewOrder(NewOrder order) {

            try {

                var request = new RestRequest("/orders")
                                .AddBody(order);

                var response = await _client.PostAsync<NewOrderResponse>(request);

                return response;

            } catch {

                return null;

            }

        }

        public async Task<NewReleaseResponse?> PostNewRelease(Guid orderId, NewRelease release) {

            try {

                var request = new RestRequest($"/orders/{orderId}/generic-cnc-releases")
                                .AddBody(release);

                var response = await _client.PostAsync<NewReleaseResponse>(request);

                return response;

            } catch {

                return null;

            }

        }

    }

    public class NewOrderResponse {
        public Guid Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Customer { get; set; } = string.Empty;
        public string Vendor { get; set; } = string.Empty;
        public DateTime OrderedDate { get; set; }
        public DateTime WantByDate { get; set; }
        public bool IsRush { get; set; }
        public decimal Price { get; set; }
        public decimal Shipping { get; set; }
        public decimal Tax { get; set; }
    }

    public class NewReleaseResponse {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public int Type { get; set; }
        public DateTime ReleaseDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public int Status { get; set; }
        public int ItemCount { get; set; }
    }

    public class NewOrder {
        public required string Number { get; set; }
        public required string Name { get; set; }
        public required string Customer { get; set; }
        public required string Vendor { get; set; }
        public required DateTime OrderedDate { get; set; }
        public required DateTime WantByDate { get; set; }
        public required bool IsRush { get; set; }
        public required decimal Price { get; set; }
        public required decimal Shipping { get; set; }
        public required decimal Tax { get; set; }
        public required string Note { get; set; }
        public required bool IsAllmoxyOrder { get; set; }
    }

    public class NewRelease {
        public required int ItemCount { get; set; }
    }

}
