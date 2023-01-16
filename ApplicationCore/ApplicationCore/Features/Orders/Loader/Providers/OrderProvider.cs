using ApplicationCore.Features.Orders.Loader.Providers.DTO;
using ApplicationCore.Features.Orders.Loader.Providers.Results;

namespace ApplicationCore.Features.Orders.Loader.Providers;

public interface IOrderProvider {

    public Task<ValidationResult> ValidateSource(string source);

    public Task<OrderData?> LoadOrderData(string source);

}
