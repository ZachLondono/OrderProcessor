using ApplicationCore.Features.Companies;
using ApplicationCore.Features.Companies.Commands;
using ApplicationCore.Features.Companies.Domain;
using ApplicationCore.Features.Companies.Domain.ValueObjects;
using ApplicationCore.Features.Companies.Queries;
using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Bus;
using FluentAssertions;
using NSubstitute;

namespace ApplicationCore.Tests.Unit.Companies;

public class CompanyStateTests {

    private readonly CompanyState _sut;
    private readonly IBus _bus = Substitute.For<IBus>();

    public CompanyStateTests() {
        _sut = new(_bus);
    }

    [Fact]
    public void ShouldBeNullByDefault() {

        // Assert
        _sut.Company.Should().BeNull();
        _sut.IsDirty.Should().BeFalse();

    }

    [Fact]
    public void ReplaceCompany_ShouldSetCopmany_AndResetIsDirty() {

        // Arrange
        var company = new Company(Guid.NewGuid(), "Company", new(), "", "", "", "", ReleaseProfile.Default, CompleteProfile.Default);

        // Act
        _sut.ReplaceCompany(company);

        // Assert
        _sut.Company.Should().Be(company);
        _sut.IsDirty.Should().BeFalse();

    }

    [Fact]
    public void UpdateCompany_ShouldDoNothing_WhenCompanyIsNotSet() {

        // Arrange
        string name = "NewName";
        Address addr = new() {
            Line1 = "ABC123"
        };
        string phoneNumber = "123-456-7890";
        string invoiceEmail = "abc@email.com";
        string confirmationEmail = "abc@email.com";
        string contactName = "Bob";

        // Act
        _sut.UpdateCompany(name, addr, phoneNumber, invoiceEmail, confirmationEmail, contactName);

        // Assert
        _sut.Company.Should().BeNull();
        _sut.IsDirty.Should().BeFalse();

    }

    [Fact]
    public void UpdateCompany_ShouldReplaceCopmany_AndSetIsDirty_WhenCompanyIsSet() {

        // Arrange
        var company = new Company(Guid.NewGuid(), "Company", new(), "", "", "", "", ReleaseProfile.Default, CompleteProfile.Default);
        _sut.ReplaceCompany(company);

        string name = "NewName";
        Address addr = new() {
            Line1 = "ABC123"
        };
        string phoneNumber = "123-456-7890";
        string invoiceEmail = "abc@email.com";
        string confirmationEmail = "abc@email.com";
        string contactName = "Bob";

        // Act
        _sut.UpdateCompany(name, addr, phoneNumber, invoiceEmail, confirmationEmail, contactName);

        // Assert
        _sut.Company.Should().NotBe(company);
        _sut.Company.Should().NotBeNull();
        _sut.Company!.Address.Should().Be(addr);
        _sut.Company.Name.Should().Be(name);
        _sut.Company.PhoneNumber.Should().Be(phoneNumber);
        _sut.Company.InvoiceEmail.Should().Be(invoiceEmail);
        _sut.Company.ConfirmationEmail.Should().Be(confirmationEmail);
        _sut.Company.Id.Should().Be(company.Id);
        _sut.IsDirty.Should().BeTrue();

    }

    [Fact]
    public void SaveChanges_ShouldDoNothing_WhenCompanyIsNotSet() {

        // Act
        _sut.SaveChanges().Wait();

        // Assert
        _sut.Company.Should().BeNull();
        _sut.IsDirty.Should().BeFalse();
        _bus.DidNotReceiveWithAnyArgs().Send(default(IQuery<object>)!);
        _bus.DidNotReceiveWithAnyArgs().Send(default(ICommand<object>)!);

    }

    [Fact]
    public void UpdateCompany_ShouldCallUpdate_AndResetIsDirty_WhenCompanyIsSet() {

        // Arrange
        var company = new Company(Guid.NewGuid(), "Company", new(), "", "", "", "", ReleaseProfile.Default, CompleteProfile.Default);
        _sut.ReplaceCompany(company);

        string name = "NewName";
        Address addr = new() {
            Line1 = "ABC123"
        };
        string phoneNumber = "";
        string invoiceEmail = "";
        string confirmationEmail = "";
        string contactName = "";
        _sut.UpdateCompany(name, addr, phoneNumber, invoiceEmail, confirmationEmail, contactName);

        // Act
        _sut.SaveChanges().Wait();

        // Assert
        _sut.IsDirty.Should().BeFalse();
        _sut.Company.Should().NotBeNull();
        _bus.Received(1).Send(new UpdateCompany.Command(_sut.Company!));

    }

    [Fact]
    public void LoadCompany_ShouldCallQuery_AndSetCompany() {

        // Arrange
        var company = new Company(Guid.NewGuid(), "Company", new(), "", "", "", "", ReleaseProfile.Default, CompleteProfile.Default);
        var response = new Response<Company?>(company);
        _bus.Send(new GetCompanyById.Query(company.Id)).Returns(response);

        // Act
        _sut.LoadCompany(company.Id).Wait();

        // Assert
        _sut.IsDirty.Should().BeFalse();
        _sut.Company.Should().Be(company);
        _bus.Received(1).Send(new GetCompanyById.Query(company.Id));

    }

}
