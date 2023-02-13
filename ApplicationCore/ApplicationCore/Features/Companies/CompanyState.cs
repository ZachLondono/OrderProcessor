using ApplicationCore.Features.Companies.Commands;
using ApplicationCore.Features.Companies.Domain;
using ApplicationCore.Features.Companies.Domain.ValueObjects;
using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Infrastructure.Bus;

namespace ApplicationCore.Features.Companies;

public class CompanyState {

    public Company? Company { get; private set; }
    public bool IsDirty { get; private set; }

    private readonly IBus _bus;

    public CompanyState(IBus bus) {
        _bus = bus;
    }

    public async Task LoadCompany(Guid companyId) {
        var result = await _bus.Send(new GetCompanyById.Query(companyId));
        result.Match(
            company => Company = company,
            error => {
                // TODO: display error
            }
        );
        IsDirty = false;
    }

    public void UpdateCompany(string name, Address address, string phoneNumber, string invoiceEmail, string confirmationEmail, string contactName) {
        if (Company is null) return;
        Company = new Company(Company.Id, name, address, phoneNumber, invoiceEmail, confirmationEmail, contactName, Company.ReleapseProfile, Company.CompleteProfile);
        IsDirty = true;
    }

    public async Task SaveChanges() {
        if (Company is null) return;
        await _bus.Send(new UpdateCompany.Command(Company));
        IsDirty = false;
    }

    public void ReplaceCompany(Company company) {
        Company = company;
        IsDirty = false;
    }

}