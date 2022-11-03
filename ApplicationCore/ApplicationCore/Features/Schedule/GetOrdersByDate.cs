using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Features.Schedule;

public class GetOrdersByDate {

    public record Query(DateTime Date) : IQuery<IEnumerable<ScheduledOrder>>;

    public class Handler : QueryHandler<Query, IEnumerable<ScheduledOrder>> {

        private readonly IDbConnectionFactory _factory;

        public Handler(IDbConnectionFactory factory) {
           _factory = factory;
        }

        public override async Task<Response<IEnumerable<ScheduledOrder>>> Handle(Query query) {

            try { 
                
                using var connection = _factory.CreateConnection();

                const string sql = "SELECT id, number, name, productiondate, status FROM orders WHERE date(productiondate) = date(@Date);";
                var orders = await connection.QueryAsync<ScheduledOrder>(sql, query);

                return new(orders);

            } catch (Exception e) {
                return new(new Error() {
                    Message = $"Exception thrown while trying to find orders scheduled for date {query.Date.ToShortDateString()}\n\nException:\n{e.Message}"
                });
            }

        }

    }

}