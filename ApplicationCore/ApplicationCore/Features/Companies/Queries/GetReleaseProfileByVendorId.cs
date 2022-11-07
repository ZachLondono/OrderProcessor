using ApplicationCore.Features.Companies.Domain.ValueObjects;
using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Features.Companies.Queries;

public class GetReleaseProfileByVendorId {

    public record Query(Guid VendorId) : IQuery<ReleaseProfile>;

    public class Handler : QueryHandler<Query, ReleaseProfile> {

        private readonly IDbConnectionFactory _factory;

        public Handler(IDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response<ReleaseProfile>> Handle(Query request) {

            using var connection = _factory.CreateConnection();
            const string query = @"SELECT
                                    generatecutlist, cutlistoutputdirectory, printcutlist, cutlisttemplatepath,
                                    generatepackinglist, packinglistoutputdirectory, printpackinglist, packinglisttemplatepath,
                                    generateinvoice, invoiceoutputdirectory, printinvoice, invoicetemplatepath,
                                    generatebol, boloutputdirectory, printbol, boltemplatefilepath,
                                    printboxlabels, boxlabelstemplatefilepath,
                                    printorderlabel, orderlabeltemplatefilepath,
                                    printaduiepylelabel, aduiepylelabeltemplatefilepath,
                                    generatecncprograms
                                FROM releaseprofiles
                                WHERE vendorid = @VendorId;";
            var profile = await connection.QuerySingleOrDefaultAsync<ReleaseProfile>(query, request);

            if (profile is null) return new(ReleaseProfile.Default);

            return new(profile);

        }
    }

}
