using ApplicationCore.Features.Companies.Contracts.Entities;

namespace ApplicationCore.Features.Companies.Contracts;

public static class CompanyDirectory {

    public delegate Task<Customer?> GetCustomerByIdAsync(Guid id);
    public delegate Task InsertCustomerAsync(Customer customer, int? allmoxyId = null);
    public delegate Task UpdateCustomerAsync(Customer customer);
    public delegate Task<Guid?> GetCustomerIdByAllmoxyIdAsync(int id);
    public delegate Task<Guid?> GetCustomerIdByNameAsync(string name);
    public delegate Task<string?> GetCustomerOrderPrefixByIdAsync(Guid id);
    public delegate Task<string?> GetCustomerWorkingDirectoryRootByIdAsync(Guid id);

    public delegate Task<Vendor?> GetVendorByIdAsync(Guid id);
    public delegate Task UpdateVendorAsync(Vendor vendor);

}