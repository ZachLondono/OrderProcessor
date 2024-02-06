using ApplicationCore.Shared.Data;

namespace Domain.Companies.ValueObjects;

public record EmailSender(string Name, string Email, string ProtectedPassword) {

    public string GetUnprotectedPassword() => UserDataProtection.Unprotect(ProtectedPassword);

}