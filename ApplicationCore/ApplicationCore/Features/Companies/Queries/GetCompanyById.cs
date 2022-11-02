using ApplicationCore.Features.Companies.Domain;
using ApplicationCore.Features.Companies.Queries.DataModels;
using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Features.Companies.Queries;

public class GetCompanyById {

    public record Query(Guid CompanyId) : IQuery<Company>;

    public class Handler : QueryHandler<Query, Company> {

        private readonly IDbConnectionFactory _factory;

        public Handler(IDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<Company>> Handle(Query request) {

            using var connection = _factory.CreateConnection();

            const string query = @"SELECT
                                        id, name, phonenumber, invoiceemail, confirmationemail, line1, line2, line3, city, state, zip, country
                                    FROM
                                        companies
                                    LEFT JOIN addresses ON companies.id = addresses.companyid
                                    WHERE id = @CompanyId;";
            
            var data = await connection.QuerySingleAsync<CompanyDataModel>(query, request);

            var company = new Company(request.CompanyId, data.Name, new() {
                Line1 = data.Line1,
                Line2 = data.Line2,
                Line3 = data.Line3,
                City = data.City,
                State = data.State,
                Zip = data.Zip,
                Country = data.Country
            }, data.PhoneNumber, data.InvoiceEmail, data.ConfirmationEmail);

            return new(company);

        }

    }

}
