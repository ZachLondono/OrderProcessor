using ApplicationCore.Features.Companies.Domain;
using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;
using MediatR;

namespace ApplicationCore.Features.Companies.Commands;

public class UpdateCompany {

    public record Command(Company Company) : ICommand;

    public class Handler : ICommandHandler<Command> {

        private readonly IDbConnectionFactory _factory;

        public Handler(IDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response> Handle(Command request) {

            using var connection = _factory.CreateConnection();

            const string command = @"BEGIN TRANSACTION;
                                        UPDATE companies
                                        SET name = @Name, phonenumber = @PhoneNumber, invoiceemail = @InvoiceEmail, confirmationemail = @ConfirmationEmail
                                        WHERE id = @Id;
                                        UPDATE addresses
                                        SET line1 = @Line1, line2 = @Line2, line3 = @Line3, city = @City, state = @State, zip = @Zip, country = @Country
                                        WHERE companyid = @Id;
                                    COMMIT;";

            await connection.ExecuteAsync(command, new {
                request.Company.Id,
                request.Company.Name,
                request.Company.PhoneNumber,
                request.Company.InvoiceEmail,
                request.Company.ConfirmationEmail,
                request.Company.Address.Line1,
                request.Company.Address.Line2,
                request.Company.Address.Line3,
                request.Company.Address.City,
                request.Company.Address.State,
                request.Company.Address.Zip,
                request.Company.Address.Country,
            });

            return new Response();

        }

    }

}
