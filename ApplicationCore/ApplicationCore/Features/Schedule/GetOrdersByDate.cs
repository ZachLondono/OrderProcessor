using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Schedule;

public class GetOrdersByDate {

    public record Query(DateTime StartDate, DateTime EndDate) : IQuery<IEnumerable<ScheduledOrder>>;

    public class Handler : QueryHandler<Query, IEnumerable<ScheduledOrder>> {

        private readonly ILogger<Handler> _logger;
        private readonly IDbConnectionFactory _factory;

        public Handler(ILogger<Handler> logger, IDbConnectionFactory factory) {
            _logger = logger;
           _factory = factory;
        }

        public override async Task<Response<IEnumerable<ScheduledOrder>>> Handle(Query query) {

            try { 
                
                using var connection = _factory.CreateConnection();

                const string sql = @"SELECT
                                        id, number, name, productiondate, status, (SELECT name FROM companies WHERE companies.id = orders.vendorid) AS vendorname, (SELECT name FROM companies WHERE companies.id = orders.customerid) AS customername
                                    FROM orders
                                    WHERE datetime(productiondate) >= datetime(@StartDate) AND datetime(productiondate) <= datetime(@EndDate);";

                var orders = await connection.QueryAsync<ScheduledOrder>(sql, query);

                return new(orders);

            } catch (Exception e) {
                _logger.LogError("Exception thrown while trying to find scheduled orders {Query} {Exception}", query, e);
                return new(new Error() {
                    Message = $"Exception thrown while trying to find orders scheduled for date {query.StartDate.ToShortDateString()} - {query.EndDate.ToShortDateString()}\n\nException:\n{e.Message}"
                });
            }

        }

    }

}
