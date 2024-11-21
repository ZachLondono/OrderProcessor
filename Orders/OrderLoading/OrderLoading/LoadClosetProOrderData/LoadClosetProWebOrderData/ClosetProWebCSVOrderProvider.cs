using OrderLoading.ClosetProCSVCutList;
using OrderLoading.ClosetProCSVCutList.PartList;
using OrderLoading.ClosetProCSVCutList.Header;

namespace OrderLoading.LoadClosetProOrderData.LoadClosetProWebOrderData;

public class ClosetProWebCSVOrderProvider : ClosetProCSVOrderProvider {

	private readonly ClosetProClientFactory _factory;

	public ClosetProWebCSVOrderProvider(ClosetProCSVReader reader, PartListProcessor partListProcessor, OrderHeaderProcessor orderHeaderProcessor, ClosetProClientFactory factory)
		: base(reader, partListProcessor, orderHeaderProcessor) {
		_factory = factory;
	}

	protected override async Task<string?> GetCSVDataFromSourceAsync(string source) {

		if (int.TryParse(source, out int designId)) {

			var client = _factory.CreateClient();

			client.OnError += (msg) => OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, msg);

			var data = await client.GetCutListDataAsync(designId);

			return data;

		} else {

			OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"Invalid design ID format '{source}'. Design ID must be an integer.");
			return null;

		}

	}

}
