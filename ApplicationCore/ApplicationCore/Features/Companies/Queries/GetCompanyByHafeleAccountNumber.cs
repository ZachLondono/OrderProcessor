using ApplicationCore.Features.Companies.Domain;
using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;

namespace ApplicationCore.Features.Companies.Queries;

public class GetCompanyByHafeleAccountNumber {

    public record Query(string HafeleAccountNumber) : IQuery<Company?>;

    public class Handler : AbstractCompanyQueryHandler<Query> {

        public Handler(IDbConnectionFactory factory) : base(factory) { }

        protected override string GetQueryString() {
            return @"SELECT
						id, name, phonenumber, invoiceemail, confirmationemail, contactname, line1, line2, line3, city, state, zip, country
					FROM
						companies
					LEFT JOIN addresses ON companies.id = addresses.companyid
					WHERE id = (
						SELECT companyid FROM hafelecompanies_companies WHERE hafeleaccountnum = @HafeleAccountNumber
                    ); ";
        }

    }

}