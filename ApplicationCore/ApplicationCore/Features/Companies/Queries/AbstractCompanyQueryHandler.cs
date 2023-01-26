using ApplicationCore.Features.Companies.Domain;
using ApplicationCore.Features.Companies.Domain.ValueObjects;
using ApplicationCore.Features.Companies.Queries.DataModels;
using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;
using System.Data;

namespace ApplicationCore.Features.Companies.Queries;

public abstract class AbstractCompanyQueryHandler<TQuery> : QueryHandler<TQuery, Company?> where TQuery : IQuery<Company?> {

    private readonly IDbConnectionFactory _factory;

    public AbstractCompanyQueryHandler(IDbConnectionFactory factory) {
        _factory = factory;
    }

    protected abstract string GetQueryString();

    public override async Task<Response<Company?>> Handle(TQuery query) {

        using var connection = _factory.CreateConnection();

        var data = await connection.QuerySingleOrDefaultAsync<CompanyDataModel>(GetQueryString(), query);

        if (data is null) return new((Company?)null);

        ReleaseProfile release = await GetReleaseProfile(connection, data.Id);
        CompleteProfile complete = await GetCompleteProfile(connection, data.Id);

        var company = new Company(data.Id, data.Name, new() {
            Line1 = data.Line1,
            Line2 = data.Line2,
            Line3 = data.Line3,
            City = data.City,
            State = data.State,
            Zip = data.Zip,
            Country = data.Country
        }, data.PhoneNumber, data.InvoiceEmail, data.ConfirmationEmail, data.ContactName, release, complete);

        return new(company);

    }

    private async Task<ReleaseProfile> GetReleaseProfile(IDbConnection connection, Guid companyId) {

        const string query = @"SELECT
                                    generatecutlist, cutlistoutputdirectory, printcutlist, cutlisttemplatepath,
                                    generatepackinglist, packinglistoutputdirectory, printpackinglist, packinglisttemplatepath,
                                    generateinvoice, invoiceoutputdirectory, printinvoice, invoicetemplatepath,
                                    generatebol, boloutputdirectory, printbol, boltemplatefilepath,
                                    printboxlabels, boxlabelstemplatefilepath,
                                    printorderlabel, orderlabeltemplatefilepath,
                                    printaduiepylelabel, aduiepylelabeltemplatefilepath,
                                    generatecncprograms, cncreportoutputdirectory,
                                    filldoororder, generatedoorprograms, doororderoutputdirectory, doorordertemplatefilepath,
                                    generatecabinetjobsummary, cabinetjobsummarytemplatefilepath, cabinetjobsummaryoutputdirectory
                                FROM releaseprofiles
                                WHERE vendorid = @VendorId;";

        var profile = await connection.QuerySingleOrDefaultAsync<ReleaseProfile>(query, new {
            VendorId = companyId
        });

        return profile ?? ReleaseProfile.Default;

    }

    private async Task<CompleteProfile> GetCompleteProfile(IDbConnection connection, Guid companyId) {

        const string query = @"SELECT
                                    emailinvoice,
                                    invoicepdfdirectory,
                                    emailsenderemail,
                                    emailsendername,
                                    emailsenderpassword
                                FROM completeprofiles
                                WHERE vendorid = @VendorId;";

        var profile = await connection.QuerySingleOrDefaultAsync<CompleteProfile>(query, new {
            VendorId = companyId
        });

        return profile ?? CompleteProfile.Default;

    }


}