using Companies.Infrastructure;
using Dapper;
using Domain.Infrastructure.Bus;

namespace Companies.AllmoxyId.Commands;

public class InsertAllmoxyId {

    public record Command(Guid CustomerId, int AllmoxyId) : ICommand;

    public class Handler : CommandHandler<Command> {

        private readonly ICompaniesDbConnectionFactory _factory;

        public Handler(ICompaniesDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response> Handle(Command command) {

            using var connection = await _factory.CreateConnection();

            int result = connection.Execute("INSERT INTO allmoxy_ids (id, customer_id) VALUES (@AllmoxyId, @CustomerId);", command);

            if (result == 0) {

                return Response.Error(new() {
                    Title = "Allmoxy ID Was Not Set",
                    Details = "No rows where changed while trying to add customer allmoxy id"
                });

            } else {

                return Response.Success();

            }


        }
    }

}
