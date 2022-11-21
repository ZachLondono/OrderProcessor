using ApplicationCore.Features.Companies.Domain;
using ApplicationCore.Features.Companies.Queries.DataModels;
using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Features.Companies.Queries;

public abstract class AbstractCompanyQueryHandler<TQuery> : QueryHandler<TQuery, Company?>  where TQuery : IQuery<Company?> {

	private readonly IDbConnectionFactory _factory;

	public AbstractCompanyQueryHandler(IDbConnectionFactory factory) {
		_factory = factory;
	}

	protected abstract string GetQueryString();

	public override async Task<Response<Company?>> Handle(TQuery query) {

		using var connection = _factory.CreateConnection();

		var data = await connection.QuerySingleOrDefaultAsync<CompanyDataModel>(GetQueryString(), query);

		if (data is null) return new((Company?)null);

		var company = new Company(data.Id, data.Name, new() {
			Line1 = data.Line1,
			Line2 = data.Line2,
			Line3 = data.Line3,
			City = data.City,
			State = data.State,
			Zip = data.Zip,
			Country = data.Country
		}, data.PhoneNumber, data.InvoiceEmail, data.ConfirmationEmail, data.ContactName);

		return new(company);

	}

}