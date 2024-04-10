namespace OrderLoading;

public interface IOrderProvider {

	public IOrderLoadWidgetViewModel? OrderLoadingViewModel { get; set; }

	public Task<OrderData?> LoadOrderData(string source);

}
