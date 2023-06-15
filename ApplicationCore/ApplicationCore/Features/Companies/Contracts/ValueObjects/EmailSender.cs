using ApplicationCore.Shared.Data;

namespace ApplicationCore.Features.Companies.Contracts.ValueObjects;

public record EmailSender(string Name, string Email, string ProtectedPassword) {

    public string GetUnprotectedPassword() => UserDataProtection.Unprotect(ProtectedPassword);

}