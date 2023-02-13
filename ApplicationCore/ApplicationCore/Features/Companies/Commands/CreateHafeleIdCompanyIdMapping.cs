using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Features.Companies.Commands;

public class CreateHafeleIdCompanyIdMapping {

    public record Command(string HafeleAccountNumber, Guid CompanyId) : ICommand;

    public class Handler : CommandHandler<Command> {

        private readonly IDbConnectionFactory _factory;

        public Handler(IDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response> Handle(Command command) {

            using var connection = _factory.CreateConnection();

            const string sql = "INSERT INTO hafelecompanies_companies (hafeleaccountnum, companyid) VALUES (@HafeleAccountNumber, @CompanyId);";

            await connection.ExecuteAsync(sql, command);

            return new Response();

        }

    }

}
