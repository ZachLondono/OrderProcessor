using ApplicationCore.Features.Companies.Domain;
using ApplicationCore.Features.Companies.Domain.ValueObjects;
using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Features.Companies.Commands;

public class CreateCompany {

    public record Command(string Name, Address Address, string PhoneNumber, string InvoiceEmail, string ConfirmationEmail) : IQuery<Company>;

    public class Handler : QueryHandler<Command, Company> {

        private readonly IDbConnectionFactory _factory;

        public Handler(IDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<Company>> Handle(Command request) {

            using var connection = _factory.CreateConnection();

            var company = Company.Create(request.Name, request.Address, request.PhoneNumber, request.InvoiceEmail, request.ConfirmationEmail);

            const string command = @"BEGIN TRANSACTION;
                                        INSERT INTO companies
                                            (id, name, phonenumber, invoiceemail, confirmationemail)
                                        VALUES
                                            (@Id, @Name, @PhoneNumber, @InvoiceEmail, @ConfirmationEmail);
                                        INSERT INTO addresses
                                            (companyid, line1, line2, line3, city, state, zip, country)
                                        VALUES
                                            (@Id, @Line1, @Line2, @Line3, @City, @State, @Zip, @Country);
                                    COMMIT;";

            await connection.ExecuteAsync(command, new {
                company.Id,
                company.Name,
                company.PhoneNumber,
                company.InvoiceEmail,
                company.ConfirmationEmail,
                company.Address.Line1,
                company.Address.Line2,
                company.Address.Line3,
                company.Address.City,
                company.Address.State,
                company.Address.Zip,
                company.Address.Country,
            });

            return new(company);

        }

    }

}
