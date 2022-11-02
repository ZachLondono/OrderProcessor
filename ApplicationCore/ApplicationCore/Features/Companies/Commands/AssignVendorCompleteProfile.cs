using ApplicationCore.Features.Companies.Domain.ValueObjects;
using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Features.Companies.Commands;

public class AssignVendorCompleteProfile {

    public record Command(Guid VendorId, CompleteProfile Profile) : ICommand;

    public class Handler : ICommandHandler<Command> {

        private readonly IDbConnectionFactory _factory;

        public Handler(IDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response> Handle(Command request) {

            using var connection = _factory.CreateConnection();

            const string existQuery = "SELECT 1 FROM releaseprofiles WHERE vendorid = @VendorId;";

            int exists = connection.QuerySingleOrDefault<int>(existQuery, request);

            string command = "";

            if (exists == 1) {

                command = @"UPDATE completeprofiles
                            SET
                                emailinvoice = @EmailInvoice
                            WHERE vendorid = @VendorId;";

            } else {

                command = @"INSERT INTO completeprofiles
                                (emailinvoice)
                            VALUES
                                (@EmailInvoice);";

            }

            await connection.ExecuteAsync(command, request.Profile);

            return new Response();

        }

    }

}