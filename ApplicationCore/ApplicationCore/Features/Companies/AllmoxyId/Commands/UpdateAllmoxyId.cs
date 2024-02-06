using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared.Data.Companies;
using Dapper;

namespace Domain.Companies.AllmoxyId.Commands;

internal class UpdateAllmoxyId {

    public record Command(Guid CustomerId, int AllmoxyId) : ICommand;

    public class Handler : CommandHandler<Command> {

        private readonly ICompaniesDbConnectionFactory _factory;

        public Handler(ICompaniesDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response> Handle(Command command) {

            using var connection = await _factory.CreateConnection();

            int result = await connection.ExecuteAsync("UPDATE allmoxy_ids SET id = @AllmoxyId WHERE customer_id = @CustomerId;", command);

            if (result == 0) {

                return Response.Error(new() {
                    Title = "Allmoxy ID Was Not Updated",
                    Details = "No rows where changed while trying to update customer allmoxy id"
                });

            } else {

                return Response.Success();

            }


        }
    }

}
