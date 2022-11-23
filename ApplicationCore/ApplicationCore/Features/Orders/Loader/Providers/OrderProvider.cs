using ApplicationCore.Features.Orders.Loader.Providers.DTO;
using ApplicationCore.Features.Orders.Loader.Providers.Results;

namespace ApplicationCore.Features.Orders.Loader.Providers;

public abstract class OrderProvider {

    public abstract Task<ValidationResult> ValidateSource(string source);

    public abstract Task<OrderData?> LoadOrderData(string source);

}
