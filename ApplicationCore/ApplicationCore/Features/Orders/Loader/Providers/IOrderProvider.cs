using ApplicationCore.Features.Orders.Loader.Providers.DTO;
using ApplicationCore.Features.Orders.Loader.Providers.Results;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.Orders.Loader.Providers;

public interface IOrderProvider {

    public IOrderLoadingViewModel? OrderLoadingViewModel { get; set; }

    public Task<ValidationResult> ValidateSource(string source);

    public Task<OrderData?> LoadOrderData(string source);

}
