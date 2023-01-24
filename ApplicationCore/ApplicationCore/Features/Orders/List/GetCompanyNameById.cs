namespace ApplicationCore.Features.Orders.List;

public static class CompanyInfo {

    public delegate Task<string?> GetCompanyNameById(Guid id);

}
