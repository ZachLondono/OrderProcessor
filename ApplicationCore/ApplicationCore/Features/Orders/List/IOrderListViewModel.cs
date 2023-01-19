namespace ApplicationCore.Features.Orders.List;

public interface IOrderListViewModel {

    public Task<string> GetCompanyName(Guid companyId);

    public Task OpenCompanyPage(Guid companyId);

}
