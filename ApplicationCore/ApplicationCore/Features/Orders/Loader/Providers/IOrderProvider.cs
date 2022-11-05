using ApplicationCore.Features.Orders.Loader.Providers.DTO;

namespace ApplicationCore.Features.Orders.Loader.Providers;

public abstract class OrderProvider {

    public abstract Task<ValidationResult> ValidateSource(string source);

    // TODO: use different return type to return error messages
    public abstract Task<OrderData?> LoadOrderData(string source);

}
