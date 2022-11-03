using ApplicationCore.Features.Companies.Domain.ValueObjects;
using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Features.Companies.Queries;

public class GetCompleteProfileByVendorId {

    public record Query(Guid VendorId) : IQuery<CompleteProfile>;

    public class Handler : QueryHandler<Query, CompleteProfile> {

        private readonly IDbConnectionFactory _factory;

        public Handler(IDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<CompleteProfile>> Handle(Query request) {

            using var connection = _factory.CreateConnection();
            const string query = @"SELECT
                                    emailinvoice,
                                    invoicepdfdirectory,
                                    emailsenderemail,
                                    emailsendername,
                                    emailsenderpassword
                                FROM completeprofiles
                                WHERE vendorid = @VendorId;";
            var profile = await connection.QuerySingleOrDefaultAsync<CompleteProfile>(query, request);

            if (profile is null) return new(new CompleteProfile());

            return new(profile);

        }
    }

}
