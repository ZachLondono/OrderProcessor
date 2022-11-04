using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Features.Companies.Commands;

public class CreateAllmoxyIdCompanyIdMapping {

    public record Command(int AllmoxyId, Guid CompanyId) : ICommand;

    public class Handler : CommandHandler<Command> {

        private readonly IDbConnectionFactory _factory;

        public Handler(IDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response> Handle(Command request) {

            using var connection = _factory.CreateConnection();

            const string command = "INSERT INTO allmoxycompanies_companies (allmoxyid, companyid) VALUES (@AllmoxyId, @CompanyId);";

            await connection.ExecuteAsync(command, request);

            return new Response();

        }
    }

}
